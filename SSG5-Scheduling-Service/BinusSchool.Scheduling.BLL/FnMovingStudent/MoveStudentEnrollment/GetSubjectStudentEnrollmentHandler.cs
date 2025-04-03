using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetSubjectStudentEnrollmentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "code", "description" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "lesson.subject.code" },
            { _columns[1], "lesson.subject.description" }
        };

        private readonly ISchedulingDbContext _dbContext;

        public GetSubjectStudentEnrollmentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentSubjectRequest>(nameof(GetStudentEnrollmentSubjectRequest.IdHomeroom));
            var predicate = PredicateBuilder.Create<MsLesson>(x => x.LessonPathways.Any(y=> y.HomeroomPathway.IdHomeroom == param.IdHomeroom));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Subject.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Subject.Description, param.SearchPattern()));

            //var query = _dbContext.Entity<MsHomeroomStudentEnrollment>()
            //    .Include(e => e.Subject)
            //    .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
            //    .SearchByIds(param)
            //    .Where(predicate)
            //    .OrderByDynamic(param, _aliasColumns);

            var query = _dbContext.Entity<MsLesson>()
                .Include(e => e.Subject)
                .Include(e => e.LessonTeachers).ThenInclude(e => e.Staff)
                .SearchByIds(param)
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns);

            var data = await query
                .Select(e => new GetSubjectStudentEnrollmentResult()
                {
                    Id = e.IdSubject,
                    //Description = e.Subject.Description + " - " + e.LessonTeachers.Where(x => x.IsPrimary).Select(e => e.Staff.FirstName == null ? e.Staff.LastName.Trim() : e.Staff.FirstName.Trim() + " " + e.Staff.LastName.Trim()).FirstOrDefault() + " - " + e.ClassIdGenerated,
                    Description = $"{e.Subject.Description} - {e.LessonTeachers.Where(x => x.IsPrimary).Select(e => e.Staff.FirstName ?? "").First()} {e.LessonTeachers.Where(x => x.IsPrimary).Select(e => e.Staff.LastName ?? "").First()} - {e.ClassIdGenerated}",
                    //subjectLevel = e.Subject.SubjectMappingSubjectLevels.Where(x=> x.IdSubject == e.IdSubject).Select(x=> new ItemValueVm
                    //{
                    //   Id = x.SubjectLevel.Id,
                    //   Description = x.SubjectLevel.Description,
                    //}).ToList()
                    IdLesson = e.Id,
                    subjectLevel = new List<ItemValueVm>()
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            var dataSubject = await _dbContext.Entity<MsSubjectMappingSubjectLevel>()
                  .Include(x => x.SubjectLevel)
                  .Where(x => data.Select(y => y.Id).Contains(x.IdSubject))
                  .OrderBy(x => x.SubjectLevel.Code)
                  .ToListAsync();

            foreach (var item in data)
            {
                if (dataSubject.Where(x=> x.IdSubject == item.Id).Count() > 0)
                {
                    item.subjectLevel.AddRange(dataSubject.Where(x => x.IdSubject == item.Id).Select(x => new ItemValueVm { Id = x.SubjectLevel.Id, Description = x.SubjectLevel.Code }).ToList());
                }    
            }

            return Request.CreateApiResult2(data as object);
        }
    }
}
