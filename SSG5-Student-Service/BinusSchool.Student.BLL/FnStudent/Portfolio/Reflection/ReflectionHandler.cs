using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection
{
    public class ReflectionHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public ReflectionHandler(IStudentDbContext studentDbContext, IMachineDateTime dateTime)
        {
            _dbContext = studentDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetReflectionComment = await _dbContext.Entity<TrReflectionStudentComment>()
               .Where(x => ids.Contains(x.IdReflectionStudent))
               .ToListAsync(CancellationToken);

            GetReflectionComment.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrReflectionStudentComment>().UpdateRange(GetReflectionComment);

            var getReflectionAttachments = await _dbContext.Entity<TrReflectionStudentAttachment>()
                .Where(x => ids.Contains(x.IdReflectionStudent))
                .ToListAsync(CancellationToken);
            if (getReflectionAttachments.Count() > 0)
            {
                getReflectionAttachments.ForEach(x => x.IsActive = false);
                _dbContext.Entity<TrReflectionStudentAttachment>().UpdateRange(getReflectionAttachments);
            }

            var GetReflectionStudent = await _dbContext.Entity<TrReflectionStudent>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetReflectionStudent.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrReflectionStudent>().UpdateRange(GetReflectionStudent);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext
                        .Entity<TrReflectionStudent>()
                        .Include(x => x.ReflectionStudentAttachments)
                        .Where(e => e.Id == id)
                        .Select(e=> new GetDetailReflectionResult
                        {
                            Id = e.Id,
                            Content = e.Content,
                            IdStudent = e.IdStudent,
                            AcademicYear = new ItemValueVm(e.IdAcademicYear,e.AcademicYear.Description),
                            Semester = e.Semester,
                            Attachments = e.ReflectionStudentAttachments.Select(x => new GetReflectionAttachment
                            {
                                Url = x.Url,
                                FileName = x.FileName,
                                FileType = x.FileType,
                                FileSize = x.FileSize,
                                FileNameOriginal = x.FileNameOriginal
                            }).ToList()
                        })
                        .SingleOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetReflectionRequest>();
            string[] _columns = { };
            
            var GetTermSemeterByDate = await (_dbContext.Entity<MsPeriod>()
                    .Include(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
                     .Where(e => e.Semester == param.Semester && e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear && param.IdSchool.Contains(e.Grade.MsLevel.MsAcademicYear.IdSchool))
                    .Select(e => new { Startdate = e.StartDate, EndDate = e.EndDate, Term = e.Code })

                    //.Where(e => e.StartDate.Date <= _dateTime.ServerTime.Date && e.EndDate.Date >= _dateTime.ServerTime.Date && param.IdSchool.Contains(e.Grade.MsLevel.MsAcademicYear.IdSchool))
                    //.Select(e=> new { Semester = e.Semester, Term = e.Code })
                ).ToListAsync(CancellationToken);

            if(GetTermSemeterByDate==null)
                throw new BadRequestException("Period is not exsis");

            var StartDate = GetTermSemeterByDate.Min(x => x.Startdate);
            var EndDate = GetTermSemeterByDate.Max(x => x.EndDate);

            // var predicate = PredicateBuilder.Create<TrReflectionStudent>(x => x.IdStudent == param.IdStudent && StartDate.Date <= x.DateIn && EndDate.Date >= x.DateIn);
            var predicate = PredicateBuilder.Create<TrReflectionStudent>(x => x.IdStudent == param.IdStudent);

            if(param.IdAcademicYear != null)
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if(param.Semester != null)
                predicate = predicate.And(x => x.Semester == param.Semester);

            var query = _dbContext.Entity<TrReflectionStudent>()
                .Include(e => e.Comments).ThenInclude(e=>e.User)
                .Include(e => e.Comments)
                .Include(e => e.ReflectionStudentAttachments)
                .Include(e=>e.Student)
                .Where(predicate)
               .OrderBy(e => e.DateIn);
           
            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReflectionResult
                {
                    Id = x.Id,
                    ReflectionContent = x.Content,
                    DateReflection = x.DateIn,
                    Term = GetTermSemeterByDate.Where(e=>e.Startdate.Date <= x.DateIn && e.EndDate.Date >= x.DateIn).Any()? GetTermSemeterByDate.Where(e => e.Startdate.Date <= x.DateIn && e.EndDate.Date >= x.DateIn).FirstOrDefault().Term:"",
                    Semester = param.Semester.ToString(),
                    ReflectionAttachments = x.ReflectionStudentAttachments.Select(e => new ReflectionAttachment
                    {
                        Url = e.Url,
                        FileName = e.FileName,
                        FileType = e.FileType,
                        FileSize = e.FileSize,
                        FileNameOriginal = e.FileNameOriginal
                    }).ToList(),
                    ReflectionCommnet = x.Comments.Select(e => new ReflectionComment
                    {
                        Id = e.Id,
                        User = $"{e.User.DisplayName}-{e.User.Username}",
                        DateComment = e.DateUp==null?e.DateIn:e.DateUp,
                        ReflectionCommet = e.Comment,
                        IsShowButton = e.IdUserComment == param.IdUser?true:false
                    })
                    .OrderByDescending(e=>e.DateComment).ToList(),
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReflectionResult
                {
                    Id = x.Id,
                    ReflectionContent = x.Content,
                    DateReflection = x.DateIn,
                    Term = GetTermSemeterByDate.Where(e => e.Startdate.Date <= x.DateIn && e.EndDate.Date >= x.DateIn).Any() ? GetTermSemeterByDate.Where(e => e.Startdate.Date <= x.DateIn && e.EndDate.Date >= x.DateIn).FirstOrDefault().Term : "",
                    Semester = param.Semester.ToString(),
                    ReflectionAttachments = x.ReflectionStudentAttachments.Select(e => new ReflectionAttachment
                    {
                        Url = e.Url,
                        FileName = e.FileName,
                        FileType = e.FileType,
                        FileSize = e.FileSize,
                        FileNameOriginal = e.FileNameOriginal
                    }).ToList(),
                    ReflectionCommnet = x.Comments.Select(e => new ReflectionComment
                    {
                        Id = e.Id,
                        User = $"{e.User.DisplayName}-{e.User.Username}",
                        DateComment = e.DateUp == null ? e.DateIn : e.DateUp,
                        ReflectionCommet = e.Comment,
                        IsShowButton = e.IdUserComment == param.IdUser ? true : false
                    }).OrderByDescending(e => e.DateComment).ToList(),
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddReflectionRequest, AddReflectionValidator>();

            var NewReflectionStudent = new TrReflectionStudent
            {
                Id = Guid.NewGuid().ToString(),
                IdStudent = body.IdStudent,
                Content = body.Content,
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester
            };

            _dbContext.Entity<TrReflectionStudent>().Add(NewReflectionStudent);

            foreach (var attachment in body.Attachments)
            {
                var newReflectionStudentAttachment = new TrReflectionStudentAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdReflectionStudent = NewReflectionStudent.Id,
                    Url = attachment.Url,
                    FileName = attachment.FileName,
                    FileNameOriginal = attachment.FileNameOriginal,
                    FileSize = attachment.FileSize,
                    FileType = attachment.FileType
                };

                _dbContext.Entity<TrReflectionStudentAttachment>().Add(newReflectionStudentAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateReflectionRequest, UpdateReflectionValidator>();

            var GetReflectionStudent = await _dbContext
                                        .Entity<TrReflectionStudent>()
                                        .Where(e => e.Id == body.Id)
                                        .SingleOrDefaultAsync(CancellationToken);

            GetReflectionStudent.Content = body.Content;
            GetReflectionStudent.IdAcademicYear = body.IdAcademicYear;
            GetReflectionStudent.Semester = body.Semester;

            _dbContext.Entity<TrReflectionStudent>().Update(GetReflectionStudent);

            var getReflectionAttachments = await _dbContext.Entity<TrReflectionStudentAttachment>()
                .Where(e => e.IdReflectionStudent == GetReflectionStudent.Id).ToListAsync(CancellationToken);
            if (getReflectionAttachments.Count() > 0)
            {
                getReflectionAttachments.ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrReflectionStudentAttachment>().UpdateRange(getReflectionAttachments);
            }

            foreach (var attachment in body.Attachments)
            {
                var newReflectionStudentAttachment = new TrReflectionStudentAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdReflectionStudent = GetReflectionStudent.Id,
                    Url = attachment.Url,
                    FileName = attachment.FileName,
                    FileNameOriginal = attachment.FileNameOriginal,
                    FileSize = attachment.FileSize,
                    FileType = attachment.FileType
                };
                _dbContext.Entity<TrReflectionStudentAttachment>().Add(newReflectionStudentAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
