using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Teaching.FnLessonPlan.WeekSetting.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting
{
    public class AddWeekSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public AddWeekSettingHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddWeekSettingRequest, AddWeekSettingValidator>();
            if (_dbContext.Entity<MsWeekSetting>().Any(x => x.IdPeriod == body.IdPeriod))
                throw new Exception("Can't create week setting because the selected term already exists");

            var period = _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.Level)
                .Where(x => x.Id == body.IdPeriod)
                .FirstOrDefault();
            if (period == null)
                throw new NotFoundException("Term not found");

            #region Find Approval Base On Level
            var msLevelApproval = await _dbContext.Entity<MsLevelApproval>()
                .Include(x => x.LessonApproval)
                    .ThenInclude(x => x.LessonApprovalStates)
                .Where(x => x.IdLevel == period.Grade.IdLevel)
                .FirstOrDefaultAsync(CancellationToken);
            
            if (msLevelApproval != null)
            {
                if (msLevelApproval.IsApproval)
                {
                    if (string.IsNullOrEmpty(msLevelApproval.IdLessonApproval))
                        throw new Exception("This level setting for approval but no approval mapping to this level, contact your admin");
                    if (msLevelApproval.LessonApproval.LessonApprovalStates.Count == 0)
                        throw new Exception("Workflow approval for this level doest have state, contact your admin");
                }
            }
            #endregion
            var now = DateTimeUtil.ServerTime;
            var IdWeekSetting = Guid.NewGuid().ToString();
            _dbContext.Entity<MsWeekSetting>().Add(new MsWeekSetting
            {
                Id = IdWeekSetting,
                IdPeriod = body.IdPeriod,
                Method = body.Method,
                Status = false,
            });

            for (int no = 0; no < body.TotalWeek; no++)
            {
                var IdWeekSettingDetail = Guid.NewGuid().ToString();

                _dbContext.Entity<MsWeekSettingDetail>().Add(new MsWeekSettingDetail
                {
                    Id = IdWeekSettingDetail,
                    IdWeekSetting = IdWeekSetting,
                    WeekNumber = no + 1,
                    Status = true,
                });

                var lessons = await _dbContext.Entity<MsLesson>()
                    .Include(x => x.Subject)
                        .ThenInclude(x => x.Grade)
                    .Include(x => x.Subject)
                        .ThenInclude(x => x.SubjectMappingSubjectLevels)
                    .Include(x => x.LessonTeachers)
                    .Where(x => x.Subject.IdGrade == period.IdGrade)
                    .Where(x => x.Semester == period.Semester)
                    .ToListAsync(CancellationToken);

                lessons = lessons.Where(x => x.LessonTeachers.Count > 0).ToList();

                foreach (var lesson in lessons)
                {
                    if (lesson.Subject.SubjectMappingSubjectLevels.Count > 0)
                    {
                        foreach (var subjectMappingSubjectLevel in lesson.Subject.SubjectMappingSubjectLevels)
                        {
                            var lessonPlan = new TrLessonPlan
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                IdWeekSettingDetail = IdWeekSettingDetail,
                                IdSubjectMappingSubjectLevel = subjectMappingSubjectLevel.Id,
                                Status = "Unsubmitted",
                            };

                            _dbContext.Entity<TrLessonPlan>().Add(lessonPlan);
                        }
                    }
                    else
                    {
                        var lessonPlan = new TrLessonPlan
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                            IdWeekSettingDetail = IdWeekSettingDetail,
                            Status = "Unsubmitted",
                        };

                        _dbContext.Entity<TrLessonPlan>().Add(lessonPlan);
                    }

                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
