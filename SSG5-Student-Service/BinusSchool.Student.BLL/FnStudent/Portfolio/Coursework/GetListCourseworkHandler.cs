using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Domain.Extensions;
using BinusSchool.Data.Api.Extensions;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework
{
    public class GetListCourseworkHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListCourseworkHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListCourseworkRequest>();

            var predicate = PredicateBuilder.Create<TrCourseworkAnecdotalStudent>(x => true);

            var dataAttachments = _dbContext.Entity<TrCourseworkAnecdotalAttachment>();
            var dataSeens = _dbContext.Entity<TrCourseworkAnecdotalStudentSeen>().Include(x => x.User);
            var dataComments = _dbContext.Entity<TrCourseworkAnecdotalStudentComment>();

            var dataUser = _dbContext.Entity<MsUser>()
                                    .Select(x => new
                                    {
                                        Id = x.Id,
                                        TeacherName = x.DisplayName
                                    });

            var dataUoi = _dbContext.Entity<MsUOI>()
                                    .Select(x => new GetListCourseworkResult
                                    {
                                        Id = x.Id,
                                        UOIName = x.Name
                                    });

            if(param.IdAcademicYear != null)
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if(param.Semester != null)
                predicate = predicate.And(x => x.Semester == param.Semester);
                    
            if(param.Type != null)
                predicate = predicate.And(x => x.Type == param.Type);

            if (!string.IsNullOrWhiteSpace(param.IdStudent))
                predicate = predicate.And(x => x.IdStudent == param.IdStudent);
            
            var query = _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                .Where(predicate)
                // .SetPagination(param)
                .Select(x => new
                {
                    Id = x.Id,
                    TeacherName = x.UserIn,
                    BinusianID = x.UserIn,
                    Userin = x.UserIn,
                    DateIn = x.DateIn,
                    DateUp = x.DateUp,
                    Date = x.DateUp == null ? x.DateIn : x.DateUp,
                    UOIName = x.IdUIO,
                    Content = x.Content,
                    CanEdit = x.UserIn == param.IdUser,
                    CanDelete = x.UserIn == param.IdUser
                });

            var dataQuery = query.OrderByDescending(x => x.Date).ToList();

            List<CourseworkAttachment> listAttachments = new List<CourseworkAttachment>();

            List<GetListCourseworkResult> result = new List<GetListCourseworkResult>();

            List<CourseworkSeenBy> listSeenBy = new List<CourseworkSeenBy>();
            
            IReadOnlyList<IItemValueVm> items = default;

            items = dataQuery
            .SetPagination(param)
            .Select(x => new GetListCourseworkResult
            {
                Id = x.Id,
                TeacherName = x.TeacherName != null ? dataUser.Where(y => y.Id == x.TeacherName).FirstOrDefault().TeacherName : null,
                BinusianID = x.BinusianID,
                Date = x.Date,
                StatusUpdated = x.DateUp == null ? false : true,
                UOIName =  x.UOIName != null ? dataUoi.Where(y => y.Id == x.UOIName).FirstOrDefault().UOIName : null,
                Content = x.Content,
                Attachments = dataAttachments.Where(y => y.IdCourseworkAnecdotalStudent == x.Id).FirstOrDefault() != null ? dataAttachments.Where(y => y.IdCourseworkAnecdotalStudent == x.Id).Select(y => new CourseworkAttachment{
                    Id = y.Id,
                    IdCourseworkAnecdotalStudent = y.IdCourseworkAnecdotalStudent,
                    Url = y.Url,
                    Filename = y.FileName,
                    Filetype = y.FileType,
                    Filesize = y.FileSize,
                    OriginalFilename = y.OriginalName
                }).ToList() : null,
                SeenBy = dataSeens.Where(y => y.IdCourseworkAnecdotalStudent == x.Id).FirstOrDefault() != null ? dataSeens.Where(y => y.IdCourseworkAnecdotalStudent == x.Id).Select(y => new CourseworkSeenBy{
                    Id = y.Id,
                    IdUserSeen = y.IdUserSeen,
                    Fullname = y.User.DisplayName
                }).Where(z => z.IdUserSeen != x.Userin).ToList() : null,
                Comments = dataComments.Where(y => y.IdCourseworkAnecdotalStudent == x.Id).FirstOrDefault() != null ? dataComments.Where(y => y.IdCourseworkAnecdotalStudent == x.Id).Select(y => new CourseworkComments{
                    Id = y.Id,
                    BinusianID = y.IdUserComment,
                    Fullname = y.User.DisplayName,
                    Date = y.DateUp == null ? y.DateIn : y.DateUp,
                    Comment = y.Comment,
                    CanEdit = y.IdUserComment == param.IdUser,
                    CanDelete = y.IdUserComment == param.IdUser
                }).OrderByDescending(e=>e.Date).ToList() : null,
                CanEdit = x.CanEdit,
                CanDelete = x.CanDelete
            }).ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
