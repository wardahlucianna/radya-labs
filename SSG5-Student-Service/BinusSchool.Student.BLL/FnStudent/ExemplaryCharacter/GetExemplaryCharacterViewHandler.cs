using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class GetExemplaryCharacterViewHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetExemplaryCharacterViewHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            //bisa apply category lebih dari 1
            // ada searching title, studentnaem, studentid

            var param = Request.ValidateParams<GetExemplaryCharacterViewRequest>(
                            nameof(GetExemplaryCharacterViewRequest.IdSchool),
                            nameof(GetExemplaryCharacterViewRequest.IdUserRequested),
                            nameof(GetExemplaryCharacterViewRequest.Type));
            string[] paramlistValue = null;
            if (param.Type == "value")
            {
                paramlistValue = param.IdValueList.Split("~");
            }


            var AcademicActived = await _dbContext.Entity<MsPeriod>()
                                    .Include(x => x.Grade).ThenInclude(y => y.MsLevel)
                                    .Where(a => a.StartDate >= DateTime.UtcNow)
                                    .Select(a => a.Grade.MsLevel.IdAcademicYear)
                                    .FirstOrDefaultAsync();

            var resultExemplary = new List<GetExemplaryCharacterViewResult>();

            var predicate = PredicateBuilder.True<TrExemplary>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    =>
                    x.ExemplaryStudents.Any(a => EF.Functions.Like(a.IdStudent, param.SearchPattern()))
                    || x.ExemplaryStudents.Any(a => EF.Functions.Like((a.Student.FirstName != null ? a.Student.FirstName + " " : ""), param.SearchPattern()))
                    || EF.Functions.Like(x.Title, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    );

            var ExemplaryCharacter = _dbContext.Entity<TrExemplary>()
                                    .Include(x => x.ExemplaryStudents).ThenInclude(y => y.Student)
                                    .Include(x => x.ExemplaryStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.MsGradePathwayClassroom).ThenInclude(y => y.Classroom)
                                    .Include(x => x.ExemplaryStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade)
                                    .Include(x => x.ExemplaryAttachments)
                                    .Include(x => x.ExemplaryLikes)
                                    .Include(x => x.TrExemplaryValues).ThenInclude(y => y.LtExemplaryValue)
                                    .Include(x => x.AcademicYear)
                                    .Where(predicate)
                                    .Where(a => (param.Type == "value" ? a.TrExemplaryValues.Any(b => paramlistValue.Contains(b.IdLtExemplaryValue)) : true)
                                    && a.AcademicYear.IdSchool == param.IdSchool)
                                    .Select(a => new GetExemplaryCharacterViewResult()
                                    {
                                        Id = a.Id,
                                        IdExemplary = a.Id,
                                        IdAcademicYear = a.IdAcademicYear,
                                        Student = (a.ExemplaryStudents.Count() > 1 ? (a.ExemplaryStudents.Count() + " Student(s)") : (a.ExemplaryStudents.Select(b => (b.Student.FirstName != null ? b.Student.FirstName + " " : "") + b.Student.LastName + " " + b.Homeroom.Grade.Code + " " + b.Homeroom.MsGradePathwayClassroom.Classroom.Code).FirstOrDefault())),
                                        Category = new ItemValueVm
                                        {
                                            Id = a.LtExemplaryCategory.Id,
                                            Description = a.LtExemplaryCategory.LongDesc
                                        },
                                        CountLikes = a.ExemplaryLikes.Count(),
                                        IsYouLiked = a.ExemplaryLikes.Where(b => b.UserIn == param.IdUserRequested).Count() > 0,
                                        Updatedby = a.UserUp,
                                        UpdatedDateView = a.DateUp != null ? ((DateTime)a.DateUp).ToString("dd MMM yyyy HH:mm") : null,
                                        Postedby = a.UserIn,
                                        PostedDate = a.PostedDate,
                                        PostedDateView = a.PostedDate.ToString("dd MMM yyyy HH:mm"),
                                        ExemplaryAttachments = a.ExemplaryAttachments.Select(b => new ExemplaryCharacterView_Attachment()
                                        {
                                            FileName = b.FileName,
                                            FileSize = b.FileSize,
                                            FileType = b.FileType,
                                            Url = b.Url
                                        }).ToList(),
                                        ValueList = a.TrExemplaryValues.Select(b => new ExemplaryCharacterView_Value()
                                        {
                                            IdExemplaryValue = b.IdLtExemplaryValue,
                                            ShortDesc = b.LtExemplaryValue.ShortDesc,
                                            LongDesc = b.LtExemplaryValue.LongDesc
                                        }).ToList()
                                    })
                                    .OrderByDescending(a => a.PostedDate);


            if (param.Type == "toprating")
            {
                ExemplaryCharacter = ExemplaryCharacter.Where(a => a.IdAcademicYear == (AcademicActived != null ? AcademicActived : a.IdAcademicYear))
                                                     .OrderByDescending(a => a.CountLikes);

            }




            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await ExemplaryCharacter
                    .Select(x => new ItemValueVm(x.Id, x.Category.Description)).ToListAsync(CancellationToken);

            else

                items = await ExemplaryCharacter
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(items.Count));


        }


        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
