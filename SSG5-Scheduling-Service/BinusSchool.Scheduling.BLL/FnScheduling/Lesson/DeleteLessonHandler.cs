using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class DeleteLessonHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public DeleteLessonHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var ids = (await GetIdsFromBody()).Distinct();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var items = await _dbContext.Entity<MsLesson>()
                .Include(x => x.AcademicYear)
                .Include(x => x.LessonTeachers)
                .Include(x => x.LessonPathways)
                .Include(x => x.HomeroomStudentEnrollments)
                .Include(x => x.AscTimetableLessons)
                .Include(x => x.Schedules).ThenInclude(x => x.AscTimetableSchedules).ThenInclude(x => x.AscTimetable).ThenInclude(x => x.AscTimetableEnrollments)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(items.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var item in items)
            {
                // also set inactive semester 2
                var item2 = default(MsLesson);
                if (item.Semester == 1)
                {
                    item2 = await _dbContext.Entity<MsLesson>()
                        .Include(x => x.LessonTeachers)
                        .Include(x => x.LessonPathways)
                        .Include(x => x.HomeroomStudentEnrollments)
                        .Include(x => x.AscTimetableLessons)
                        .Include(x => x.Schedules).ThenInclude(x => x.AscTimetableSchedules).ThenInclude(x => x.AscTimetable).ThenInclude(x => x.AscTimetableEnrollments)
                        .FirstOrDefaultAsync(x
                            => x.Id != item.Id
                            && x.IdAcademicYear == item.IdAcademicYear
                            && x.Semester == 1
                            && x.IdGrade == item.IdGrade
                            && x.IdSubject == item.IdSubject
                            && x.ClassIdGenerated == item.ClassIdGenerated,
                            CancellationToken);
                }

                foreach (var item3 in new[] { item, item2 }.Where(x => x != null))
                {
                    // set inactive lesson, enrollment & schedule
                    item.IsActive = false;
                    _dbContext.Entity<MsLesson>().Update(item);

                    foreach (var lessonTeacher in item3.LessonTeachers)
                    {
                        lessonTeacher.IsActive = false;
                        _dbContext.Entity<MsLessonTeacher>().Update(lessonTeacher);
                    }

                    foreach (var lessonPathway in item3.LessonPathways)
                    {
                        lessonPathway.IsActive = false;
                        _dbContext.Entity<MsLessonPathway>().Update(lessonPathway);
                    }

                    foreach (var enrollment in item.HomeroomStudentEnrollments)
                    {
                        enrollment.IsActive = false;
                        _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrollment);
                    }

                    foreach (var ascLesson in item.AscTimetableLessons)
                    {
                        ascLesson.IsActive = false;
                        _dbContext.Entity<TrAscTimetableLesson>().Update(ascLesson);
                    }

                    foreach (var schedule in item.Schedules)
                    {
                        schedule.IsActive = false;
                        _dbContext.Entity<MsSchedule>().Update(schedule);

                        foreach (var ascSchedule in schedule.AscTimetableSchedules)
                        {
                            ascSchedule.IsActive = false;
                            _dbContext.Entity<TrAscTimetableSchedule>().Update(ascSchedule);

                            foreach (var ascEnrollment in ascSchedule.AscTimetable.AscTimetableEnrollments)
                            {
                                ascEnrollment.IsActive = false;
                                _dbContext.Entity<TrAscTimetableEnrollment>().Update(ascEnrollment);
                            }
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = items.Select(e=>e.AcademicYear.IdSchool).Distinct().FirstOrDefault();
            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });

            return ProceedDeleteResult(undeleted.AsErrors());
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
