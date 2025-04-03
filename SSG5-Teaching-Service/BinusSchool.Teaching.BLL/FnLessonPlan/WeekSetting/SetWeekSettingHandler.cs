using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Teaching.FnLessonPlan.WeekSetting.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting
{
    public class SetWeekSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public SetWeekSettingHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetWeekSettingRequest, SetWeekSettingValidator>();

            var weekSetting = _dbContext.Entity<MsWeekSetting>()
                .Include(x => x.WeekSettingDetails)
                    .ThenInclude(X => X.LessonPlans)
                .Include(x => x.Period)
                .FirstOrDefault(x => x.Id == body.IdWeekSetting);
            if (weekSetting == null)
                throw new NotFoundException("Week Setting not found");

            if (weekSetting.WeekSettingDetails.Any(y => y.DeadlineDate != null))
            {
                if (body.TotalWeek < weekSetting.WeekSettingDetails.Where(x => x.DeadlineDate != null).Count())
                    throw new Exception("Can't set total week under total date saved");
            }

            // if (weekSetting.WeekSettingDetails.Any(x => DateTime.Now >= x.DeadlineDate))
            //     throw new Exception("Can't set week setting because it's already on progress");

            // if (weekSetting.WeekSettingDetails.Any(x => x.LessonPlans.Any(y => y.Status != "Unsubmitted")))
            //     throw new Exception("Can't set week setting because teacher already uploaded lesson plan to this week setting");

            weekSetting.Method = body.Method;

            var deleteWeekSetting = weekSetting.WeekSettingDetails
                .Where(x => x.WeekNumber > body.TotalWeek)
                .Select(x => x.Id)
                .ToList();
            foreach (var wsId in deleteWeekSetting)
            {
                var wsd = _dbContext.Entity<MsWeekSettingDetail>()
                    .Include(x => x.LessonPlans)
                    .FirstOrDefault(x => x.Id == wsId);
                wsd.IsActive = false;

                foreach (var lesson in wsd.LessonPlans)
                {
                    var l = _dbContext.Entity<TrLessonPlan>().FirstOrDefault(x => x.Id == lesson.Id);
                    if (l == null)
                        throw new NotFoundException("Lesson plan not found");
                    l.IsActive = false;
                }
            }

            for (var i = 1; i <= body.TotalWeek; i++)
            {
                var isWsExists = weekSetting.WeekSettingDetails.Where(x => x.WeekNumber == i).FirstOrDefault();
                if (isWsExists == null)
                {
                    var idWeekSettingDetail = Guid.NewGuid().ToString();
                    _dbContext.Entity<MsWeekSettingDetail>().Add(new MsWeekSettingDetail
                    {
                        Id = idWeekSettingDetail,
                        IdWeekSetting = weekSetting.Id,
                        WeekNumber = i,
                        Status = true
                    });

                    var lessons = await _dbContext.Entity<MsLesson>()
                        .Include(x => x.Subject)
                            .ThenInclude(x => x.Grade)
                        .Include(x => x.Subject)
                            .ThenInclude(x => x.SubjectMappingSubjectLevels)
                        .Include(x => x.LessonTeachers)
                        .Where(x => x.Subject.IdGrade == weekSetting.Period.IdGrade)
                        .Where(x => x.Semester == weekSetting.Period.Semester)
                        .ToListAsync(CancellationToken);

                    foreach (var lesson in lessons)
                    {
                        if (lesson.Subject.SubjectMappingSubjectLevels.Count > 0)
                        {
                            foreach (var subjectMappingSubjectLevel in lesson.Subject.SubjectMappingSubjectLevels)
                            {
                                if (!lesson.LessonTeachers.Any(x => x.IsPrimary && x.IdLesson == lesson.Id))
                                    throw new NotFoundException("Data is out of sync. please contact admin.");
                                var lessonPlan = new TrLessonPlan
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                    IdWeekSettingDetail = idWeekSettingDetail,
                                    IdSubjectMappingSubjectLevel = subjectMappingSubjectLevel.Id,
                                    Status = "Unsubmitted",
                                };

                                _dbContext.Entity<TrLessonPlan>().Add(lessonPlan);
                            }
                        }
                        else
                        {
                            if (!lesson.LessonTeachers.Any(x => x.IsPrimary && x.IdLesson == lesson.Id))
                                throw new NotFoundException("Data is out of sync. please contact admin.");
                            var lessonPlan = new TrLessonPlan
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                IdWeekSettingDetail = idWeekSettingDetail,
                                Status = "Unsubmitted",
                            };

                            _dbContext.Entity<TrLessonPlan>().Add(lessonPlan);
                        }

                    }
                }
                else
                {
                    //check data anomali
                    var lessonPlan = await _dbContext.Entity<TrLessonPlan>()
                                    .Include(x=> x.LessonTeacher).ThenInclude(x=> x.Lesson)
                                    .Include(x => x.WeekSettingDetail)
                                    .Where(x => x.WeekSettingDetail.IdWeekSetting == weekSetting.Id)
                                    .ToListAsync();

                    if (lessonPlan.Count == 0)
                    {
                        var lessons = await _dbContext.Entity<MsLesson>()
                        .Include(x => x.Subject)
                            .ThenInclude(x => x.Grade)
                        .Include(x => x.Subject)
                            .ThenInclude(x => x.SubjectMappingSubjectLevels)
                        .Include(x => x.LessonTeachers)
                        .Where(x => x.Subject.IdGrade == weekSetting.Period.IdGrade)
                        .Where(x => x.Semester == weekSetting.Period.Semester)
                        .ToListAsync(CancellationToken);

                        foreach (var lesson in lessons)
                        {
                            if (lesson.Subject.SubjectMappingSubjectLevels.Count > 0)
                            {
                                foreach (var subjectMappingSubjectLevel in lesson.Subject.SubjectMappingSubjectLevels)
                                {
                                    if (!lesson.LessonTeachers.Any(x => x.IsPrimary && x.IdLesson == lesson.Id))
                                        throw new NotFoundException("Data is out of sync. please contact admin.");
                                    var lessonPlanNew = new TrLessonPlan
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                        IdWeekSettingDetail = isWsExists.Id,
                                        IdSubjectMappingSubjectLevel = subjectMappingSubjectLevel.Id,
                                        Status = "Unsubmitted",
                                    };

                                    _dbContext.Entity<TrLessonPlan>().Add(lessonPlanNew);
                                }
                            }
                            else
                            {
                                if (!lesson.LessonTeachers.Any(x => x.IsPrimary && x.IdLesson == lesson.Id))
                                    throw new NotFoundException("Data is out of sync. please contact admin.");
                                var lessonPlanNew = new TrLessonPlan
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                    IdWeekSettingDetail = isWsExists.Id,
                                    Status = "Unsubmitted",
                                };

                                _dbContext.Entity<TrLessonPlan>().Add(lessonPlanNew);
                            }
                        }
                    }
                    else
                    {
                        var lessons = await _dbContext.Entity<MsLesson>()
                                    .Include(x => x.Subject)
                                        .ThenInclude(x => x.Grade)
                                    .Include(x => x.Subject)
                                        .ThenInclude(x => x.SubjectMappingSubjectLevels)
                                    .Include(x => x.LessonTeachers)
                                    .Where(x => x.Subject.IdGrade == weekSetting.Period.IdGrade)
                                    .Where(x => x.Semester == weekSetting.Period.Semester)
                                    .Where(x => !lessonPlan.Select(e=> e.LessonTeacher.Lesson.Id).ToList().Contains(x.Id))
                                    .ToListAsync(CancellationToken);

                        foreach (var lesson in lessons)
                        {
                            if (lesson.Subject.SubjectMappingSubjectLevels.Count > 0)
                            {
                                foreach (var subjectMappingSubjectLevel in lesson.Subject.SubjectMappingSubjectLevels)
                                {
                                    if (!lesson.LessonTeachers.Any(x => x.IsPrimary && x.IdLesson == lesson.Id))
                                        throw new NotFoundException("Data is out of sync. please contact admin.");
                                    var lessonPlanNew = new TrLessonPlan
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                        IdWeekSettingDetail = isWsExists.Id,
                                        IdSubjectMappingSubjectLevel = subjectMappingSubjectLevel.Id,
                                        Status = "Unsubmitted",
                                    };

                                    _dbContext.Entity<TrLessonPlan>().Add(lessonPlanNew);
                                }
                            }
                            else
                            {
                                if (!lesson.LessonTeachers.Any(x => x.IsPrimary && x.IdLesson == lesson.Id))
                                    throw new NotFoundException("Data is out of sync. please contact admin.");
                                var lessonPlanNew = new TrLessonPlan
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLessonTeacher = lesson.LessonTeachers.Where(x => x.IsPrimary).FirstOrDefault().Id,
                                    IdWeekSettingDetail = isWsExists.Id,
                                    Status = "Unsubmitted",
                                };

                                _dbContext.Entity<TrLessonPlan>().Add(lessonPlanNew);
                            }
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
