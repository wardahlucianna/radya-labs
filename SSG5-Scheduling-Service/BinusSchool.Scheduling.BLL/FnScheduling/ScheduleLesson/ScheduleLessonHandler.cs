using System;
using System.Collections.Generic;
using System.Text;
using System;
using BinusSchool.Common.Functions.Handler;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Common.Extensions;
using System.Linq;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleLesson;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleLesson
{
    public class ScheduleLessonHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public ScheduleLessonHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }


        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<ScheduleLessonRequest>();
            var listGeneratedScheduleLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(e => e.Lesson).ThenInclude(e=>e.Grade)
                                            .Where(e =>
                                                e.Lesson.IdAcademicYear == param.idAcademicYear
                                                && e.IsGenerated
                                            )
                                            .ToListAsync(CancellationToken);

            var listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                        .Where(e => e.IdAcademicYear == param.idAcademicYear)
                                        .ToListAsync(CancellationToken);

            var listSchedule = await _dbContext.Entity<MsSchedule>()
                                .Include(e => e.Lesson)
                                .Include(e=>e.WeekVarianDetail)
                                .Where(e =>
                                    e.Lesson.IdAcademicYear == param.idAcademicYear
                                )
                                .ToListAsync(CancellationToken);

            var listDay = await _dbContext.Entity<LtDay>()
                                .ToListAsync(CancellationToken);

            var listGeneratedScheduleLessonGroup = listGeneratedScheduleLesson
             .Select(e => new
             {
                 scheduleDate = e.ScheduleDate,
                 idUser = e.IdUser,
                 idLesson = e.IdLesson,
                 idSubject = e.IdSubject,
                 idSession = e.IdSession,
             })
             .Distinct()
             .ToList();

            foreach (var item in listGeneratedScheduleLessonGroup)
            {
                var generatedScheduleLessonByItem = listGeneratedScheduleLesson
                            .Where(e =>
                                    e.ScheduleDate == item.scheduleDate
                                    && e.IdUser == item.idUser
                                    && e.IdLesson == item.idLesson
                                    && e.IdSession == item.idSession
                                    && e.IdSubject == item.idSubject
                                    ).OrderBy(e => e.DateIn).FirstOrDefault();

                if (generatedScheduleLessonByItem == null)
                    continue;

                var dayByItem = listDay.Where(e => e.Description == generatedScheduleLessonByItem.DaysOfWeek).FirstOrDefault();

                var scheduleByItem = listSchedule
                                        .Where(e =>
                                            e.IdUser == generatedScheduleLessonByItem.IdUser
                                            && e.IdLesson == generatedScheduleLessonByItem.IdLesson
                                            && e.IdSession == generatedScheduleLessonByItem.IdSession
                                            && e.WeekVarianDetail.IdWeek == generatedScheduleLessonByItem.IdWeek
                                            && e.IdDay == dayByItem.Id
                                        ).FirstOrDefault();
                if (scheduleByItem == null)
                    continue;

                var scheduleLessonByItem = listScheduleLesson
                            .Where(e =>
                                    e.ScheduleDate == item.scheduleDate
                                    && e.IdLesson == item.idLesson
                                    && e.IdSession == item.idSession
                                    && e.IdSubject == item.idSubject
                                    ).OrderBy(e => e.DateIn).FirstOrDefault();

                if (scheduleLessonByItem != null)
                    continue;

                MsScheduleLesson newSacheduleLesson = new MsScheduleLesson
                {
                    Id = generatedScheduleLessonByItem.Id,
                    IdWeek = generatedScheduleLessonByItem.IdWeek,
                    ScheduleDate = generatedScheduleLessonByItem.ScheduleDate,
                    IdVenue = generatedScheduleLessonByItem.IdVenue,
                    VenueName = generatedScheduleLessonByItem.VenueName,
                    ClassID = generatedScheduleLessonByItem.ClassID,
                    IdLesson = generatedScheduleLessonByItem.IdLesson,
                    IsGenerated = generatedScheduleLessonByItem.IsGenerated,
                    DaysOfWeek = generatedScheduleLessonByItem.DaysOfWeek,
                    EndTime = generatedScheduleLessonByItem.EndTime,
                    IdSession = generatedScheduleLessonByItem.IdSession,
                    IdSubject = generatedScheduleLessonByItem.IdSubject,
                    SessionID = generatedScheduleLessonByItem.SessionID,
                    StartTime = generatedScheduleLessonByItem.StartTime,
                    SubjectName = generatedScheduleLessonByItem.SubjectName,
                    IdAcademicYear = generatedScheduleLessonByItem.Lesson.IdAcademicYear,
                    IdLevel = generatedScheduleLessonByItem.Lesson.Grade.IdLevel,
                    IdGrade = generatedScheduleLessonByItem.Lesson.IdGrade,
                    IdDay = dayByItem.Id,
                };

                _dbContext.Entity<MsScheduleLesson>().Add(newSacheduleLesson);
            }
            await _dbContext.SaveChangesAsync();
            return Request.CreateApiResult2();
        }
    }
}
