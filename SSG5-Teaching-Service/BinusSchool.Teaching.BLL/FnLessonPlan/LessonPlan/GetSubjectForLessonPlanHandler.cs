using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class GetSubjectForLessonPlanHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetSubjectForLessonPlanHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectForLessonPlanRequest>(
                nameof(GetSubjectForLessonPlanRequest.IdUser),
                nameof(GetSubjectForLessonPlanRequest.IdAcademicYear),
                nameof(GetSubjectForLessonPlanRequest.IdLevel),
                nameof(GetSubjectForLessonPlanRequest.IdGrade),
                nameof(GetSubjectForLessonPlanRequest.Semester)

            );

            var getDataSubjectForLessonPlan =
                await (
                    from _subject in _dbContext.Entity<MsSubject>()
                    join _subjectMapping in _dbContext.Entity<MsSubjectMappingSubjectLevel>() on _subject.Id equals _subjectMapping.IdSubject into subjectMapping
                    from _subjectMapping in subjectMapping.DefaultIfEmpty()
                    join _subjectLevel in _dbContext.Entity<MsSubjectLevel>() on _subjectMapping.IdSubjectLevel equals _subjectLevel.Id
                        into subjectLevel
                    from _subjectLevel in subjectLevel.DefaultIfEmpty()
                    join _lesson in _dbContext.Entity<MsLesson>() on _subject.Id equals _lesson.IdSubject
                    join _grade in _dbContext.Entity<MsGrade>() on _subject.IdGrade equals _grade.Id
                    join _level in _dbContext.Entity<MsLevel>() on _grade.IdLevel equals _level.Id
                    join _lessonTeacher in _dbContext.Entity<MsLessonTeacher>() on _lesson.Id equals _lessonTeacher.IdLesson
                    where _subject.IsNeedLessonPlan == true
                        && _level.AcademicYear.Id == param.IdAcademicYear 
                        && _level.Id == param.IdLevel
                        && _grade.Id == param.IdGrade
                        && _lessonTeacher.IdUser == param.IdUser
                        && _lesson.Semester == param.Semester
                        && (param.Search!=null?_subject.Description.Contains(param.Search):true)
                    orderby _subject.Description
                    select new GetSubjectForLessonPlanResult
                    {
                        Id = _subject.Id,
                        Code = _subject.Code,
                        Description = _subject.Description,
                        SubjectLevel = _subjectMapping == null || _subjectLevel ==null ? null : _subjectLevel.Code,
                        IdSubjectMappingSubjectLevel = _subjectMapping == null ? null : _subjectMapping.Id
                    }
                    ).Distinct().AsNoTracking().ToListAsync(CancellationToken);

            return Request.CreateApiResult2(getDataSubjectForLessonPlan as object);
        }
    }
}
