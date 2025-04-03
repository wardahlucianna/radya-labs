using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class GetDetailLessonPlanInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetDetailLessonPlanInformationHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailLessonPlanInformationRequest>(
                nameof(GetDetailLessonPlanInformationRequest.IdLessonPlan)
            );

            var lessonPlan = _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.WeekSettingDetail)
                    .ThenInclude(x => x.WeekSetting)
                        .ThenInclude(x => x.Period)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                                    .ThenInclude(x => x.AcademicYear)
                .Include(x => x.LessonTeacher)
                    .ThenInclude(x => x.Lesson)
                        .ThenInclude(x => x.Subject)
                .Include(x => x.LessonTeacher)
                        .ThenInclude(x => x.Staff)
                .Include(x => x.SubjectMappingSubjectLevel)
                    .ThenInclude(x => x.SubjectLevel)
                .FirstOrDefault(x => x.Id == param.IdLessonPlan);
            if (lessonPlan == null)
                throw new NotFoundException("Lesson plan document not found");
            
            var data = new GetDetailLessonPlanInformationResult
            {
                AcademicYear = lessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.Level.AcademicYear.Code,
                Level = lessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.Level.Code,
                Grade = lessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.Description,
                Term = lessonPlan.WeekSettingDetail.WeekSetting.Period.Description,
                IdSubject = lessonPlan.LessonTeacher.Lesson.Subject.Id,
                Subject = lessonPlan.LessonTeacher.Lesson.Subject.Description,
                SubjectLevel = lessonPlan.SubjectMappingSubjectLevel != null ? lessonPlan.SubjectMappingSubjectLevel.SubjectLevel.Code : "-",
                Periode = "Week " + lessonPlan.WeekSettingDetail.WeekNumber,
            };
                
            return Task.FromResult(Request.CreateApiResult2(data as object));
        }
    }
}