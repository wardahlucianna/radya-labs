using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class DeleteHomeroomHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;
        public DeleteHomeroomHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var ids = (await GetIdsFromBody()).Distinct();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var homerooms = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.AcademicYear)
                .Include(x => x.HomeroomStudents)
                .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.LessonTeachers)
                .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.HomeroomStudentEnrollments)
                .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.AscTimetableLessons)
                .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.Schedules)
                    .ThenInclude(x => x.AscTimetableSchedules).ThenInclude(x => x.AscTimetable).ThenInclude(x => x.AscTimetableEnrollments)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(homerooms.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var homeroom in homerooms)
            {
                // also delete semester 2
                var homeroom2 = default(MsHomeroom);
                if (homeroom.Semester == 1)
                {
                    homeroom2 = await _dbContext.Entity<MsHomeroom>()
                        .Include(x => x.HomeroomStudents)
                        .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.LessonTeachers)
                        .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.HomeroomStudentEnrollments)
                        .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.AscTimetableLessons)
                        .Include(x => x.HomeroomPathways).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson).ThenInclude(x => x.Schedules)
                            .ThenInclude(x => x.AscTimetableSchedules).ThenInclude(x => x.AscTimetable).ThenInclude(x => x.AscTimetableEnrollments)
                        .FirstOrDefaultAsync(x
                            => x.IdAcademicYear == homeroom.IdAcademicYear
                            && x.Semester == 2
                            && x.IdGrade == homeroom.IdGrade
                            && x.IdGradePathwayClassRoom == homeroom.IdGradePathwayClassRoom
                            && x.IdGradePathway == homeroom.IdGradePathway,
                            CancellationToken);
                }

                foreach (var homeroom3 in new[] { homeroom, homeroom2 }.Where(x => x != null))
                {
                    // don't set inactive when row have to-many relation
                    // don't set already use for second homeroom (that have semester 2)
                    if (homeroom.HomeroomStudents.Count != 0 && homeroom.Semester == homeroom3.Semester)
                    {
                        undeleted.AlreadyUse ??= new Dictionary<string, string>();
                        undeleted.AlreadyUse.Add(homeroom.Id, string.Format(Localizer["ExAlreadyUse"], homeroom.Id));
                    }
                    else
                    {
                        homeroom.IsActive = false;
                        _dbContext.Entity<MsHomeroom>().Update(homeroom);

                        // delete homeroom pathway
                        foreach (var hrPathway in homeroom.HomeroomPathways)
                        {
                            hrPathway.IsActive = false;
                            _dbContext.Entity<MsHomeroomPathway>().Update(hrPathway);

                            // delete lesson pathway
                            foreach (var lessonPathway in hrPathway.LessonPathways)
                            {
                                lessonPathway.Lesson.IsActive = false;
                                _dbContext.Entity<MsLesson>().Update(lessonPathway.Lesson);

                                // delete lesson teacher
                                foreach (var lessonTeacher in lessonPathway.Lesson.LessonTeachers)
                                {
                                    lessonTeacher.IsActive = false;
                                    _dbContext.Entity<MsLessonTeacher>().Update(lessonTeacher);
                                }

                                // delete student enrollment
                                foreach (var enrollment in lessonPathway.Lesson.HomeroomStudentEnrollments)
                                {
                                    enrollment.IsActive = false;
                                    _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrollment);
                                }

                                // delete asc timetable lesson
                                foreach (var ascLesson in lessonPathway.Lesson.AscTimetableLessons)
                                {
                                    ascLesson.IsActive = false;
                                    _dbContext.Entity<TrAscTimetableLesson>().Update(ascLesson);
                                }

                                // delete schedule
                                foreach (var schedule in lessonPathway.Lesson.Schedules)
                                {
                                    schedule.IsActive = false;
                                    _dbContext.Entity<MsSchedule>().Update(schedule);

                                    // delete asc schedule
                                    foreach (var ascSchedule in schedule.AscTimetableSchedules)
                                    {
                                        ascSchedule.IsActive = false;
                                        _dbContext.Entity<TrAscTimetableSchedule>().Update(ascSchedule);

                                        // delete asc enrollment
                                        foreach (var ascEnrollment in ascSchedule.AscTimetable.AscTimetableEnrollments)
                                        {
                                            ascEnrollment.IsActive = false;
                                            _dbContext.Entity<TrAscTimetableEnrollment>().Update(ascEnrollment);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = homerooms.Select(e => e.AcademicYear.IdSchool).Distinct().FirstOrDefault();
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
