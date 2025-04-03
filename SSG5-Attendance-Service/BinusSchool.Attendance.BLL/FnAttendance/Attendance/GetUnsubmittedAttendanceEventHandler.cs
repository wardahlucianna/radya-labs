using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetUnsubmittedAttendanceEventHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        public GetUnsubmittedAttendanceEventHandler(IAttendanceDbContext dbContext, IMachineDateTime Datetime)
        {
            _dbContext = dbContext;
            _datetime = Datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnsubmittedAttendanceEventRequest>();

            var listEvent = await _dbContext.Entity<TrEvent>()
                            .Include(e => e.EventDetails).ThenInclude(e => e.UserEvents)
                            .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAtdPICStudents)
                            .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAtdCheckStudents)
                            .Where(x => x.IdAcademicYear == param.idAcademicYear
                                        && x.EventIntendedFor.Any(e => e.IntendedFor == "STUDENT")
                                        && x.EventIntendedFor.Any(e=>e.EventIntendedForAttendanceStudents.Any(e=>e.IsSetAttendance))
                                        && x.EventIntendedFor.Any(e=>e.EventIntendedForAttendanceStudents.Any(e=>e.Type== EventIntendedForAttendanceStudent.Mandatory))
                                        && x.EventDetails.Any(e => e.StartDate.Date <= _datetime.ServerTime.Date)
                                        && x.StatusEvent== "Approved"
                                        && x.EventIntendedFor.Any(e=>e.EventIntendedForAttendanceStudents.Any(g=>g.EventIntendedForAtdPICStudents.Any(f=>f.IdUser==param.idUser)))
                                        && x.Id == "31bd6924-1af9-44a0-8c8e-45d5f2e4ef9d"
                                        )
                            .AsNoTracking().ToListAsync(CancellationToken);

            var listIdStudent = listEvent.SelectMany(e => e.EventDetails.SelectMany(f => f.UserEvents.Select(g => g.IdUser))).ToList();

            List<DateTime> listDateEvent = new List<DateTime>();
            foreach (var itemEvent in listEvent.SelectMany(e => e.EventDetails).ToList())
            {
                var startDate = itemEvent.StartDate.Date;
                var endDate = itemEvent.EndDate.Date;

                for (var day = startDate; day.Date <= endDate; day = day.AddDays(1))
                {
                    if (!listDateEvent.Where(e => e == day).Any())
                        listDateEvent.Add(day);
                }
            }

            var listGeneratedScheduleLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                        .Include(e=>e.Lesson).ThenInclude(e=>e.LessonTeachers)
                        .Where(x => listDateEvent.Contains(x.ScheduleDate.Date)
                                && x.Lesson.IdAcademicYear == param.idAcademicYear
                                && listIdStudent.Contains(x.GeneratedScheduleStudent.IdStudent)
                            )
                        .Select(e => new
                        {
                            e.Id,
                            e.IdStudent,
                            e.IdHomeroom,
                            e.ScheduleDate,
                            e.Lesson,
                            e.IdLesson
                        })
                        .AsNoTracking().ToListAsync();

            var listIdEvent = listEvent.Select(e=>e.Id).ToList();

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                           .Where(x => x.Homeroom.IdAcademicYear == param.idAcademicYear && listIdStudent.Contains(x.IdStudent))
                           .Select(e => new
                           {
                               idHomeroom = e.IdHomeroom,
                               idStudent  = e.IdStudent
                           })
                           .AsNoTracking().ToListAsync();

            var listIdHomeroom = listHomeroomStudent.Select(e => e.idHomeroom).ToList();

            #region homeroom teacher
            var listIdHomeroomByHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                           .Where(x => x.Homeroom.IdAcademicYear == param.idAcademicYear && x.IdBinusian == param.idUser && listIdHomeroom.Contains(x.IdHomeroom))
                           .Select(e => e.IdHomeroom)
                           .AsNoTracking().ToListAsync();
            #endregion

            #region subject teacher
            var listIdLesson = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                           .Where(x => x.Lesson.IdAcademicYear == param.idAcademicYear && listIdStudent.Contains(x.HomeroomStudent.IdStudent))
                           .Select(e => e.IdLesson)
                           .AsNoTracking().ToListAsync();

            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                           .Include(e=>e.Lesson)
                           .Where(x => x.Lesson.IdAcademicYear == param.idAcademicYear && x.IdUser == param.idUser && listIdLesson.Contains(x.IdLesson))
                           .Select(e => new
                           {
                               e.IdLesson,
                               e.Lesson.IdSubject
                           })
                           .AsNoTracking().ToListAsync();

            var listIdLessonByLessonTeacher = listLessonTeacher.Select(e => e.IdLesson).ToList();
            #endregion

            var listIdGeneratedScheduleLesson = listGeneratedScheduleLesson.Where(e=> listDateEvent.Contains(e.ScheduleDate.Date)).Select(e => e.Id).Distinct().ToList();

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntry>()
                      .Include(e => e.GeneratedScheduleLesson)
                      .Where(x => listIdGeneratedScheduleLesson.Contains(x.IdGeneratedScheduleLesson))
                      .Select(e => new
                      {
                          e.Id,
                          e.IdGeneratedScheduleLesson,
                          e.Status,
                          e.GeneratedScheduleLesson.IdStudent
                      })
                      .AsNoTracking().ToListAsync();

            List<GetUnsubmittedAttendanceEventResult> result = new List<GetUnsubmittedAttendanceEventResult>();
            foreach (var itemEvent in listEvent)
            {
               var listEventDate = itemEvent.EventDetails.Select(e => new
                {
                    e.StartDate,
                    e.EndDate
                }).ToList();

                var listIdStudentByEvent = itemEvent.EventDetails.SelectMany(e => e.UserEvents.Select(d => d.IdUser)).ToList();

                var picType = itemEvent.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.SelectMany(f => f.EventIntendedForAtdPICStudents.Select(g => g.Type))).FirstOrDefault();

                foreach (var itemDateEvent in listEventDate)
                {
                    bool isAttendance = true;

                    if(picType == EventIntendedForAttendancePICStudent.Homeroom)
                        listGeneratedScheduleLesson = listGeneratedScheduleLesson.Where(e => listIdHomeroomByHomeroomTeacher.Contains(e.IdHomeroom)).ToList();

                    if (picType == EventIntendedForAttendancePICStudent.Subject)
                        listGeneratedScheduleLesson = listGeneratedScheduleLesson.Where(e => listIdLessonByLessonTeacher.Contains(e.IdLesson)).ToList();

                    var listGeneratedScheduleLessonbyDate = listGeneratedScheduleLesson
                                                            .Where(e => (e.ScheduleDate >= itemDateEvent.StartDate && e.ScheduleDate <= itemDateEvent.EndDate)
                                                                        && listIdStudentByEvent.Contains(e.IdStudent))
                                                            .Select(e => e.Id)
                                                            .ToList();

                    if (!listGeneratedScheduleLessonbyDate.Any())
                        continue;

                    var listAttendanceEntryByIdGenerated = listAttendanceEntry
                                                        .Where(e => listGeneratedScheduleLessonbyDate.Contains(e.IdGeneratedScheduleLesson))
                                                        .Select(e=>e.IdGeneratedScheduleLesson)
                                                        .ToList();

                    var listGeneratedScheduleLessonExceptAttendanceEntry= listGeneratedScheduleLessonbyDate.Where(e=> !listAttendanceEntryByIdGenerated.Contains(e)).Any();


                    if (listGeneratedScheduleLessonExceptAttendanceEntry)
                    {
                        var CheckName = itemEvent.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.SelectMany(f => f.EventIntendedForAtdCheckStudents.Select(h => h.CheckName).ToList()).ToList()).FirstOrDefault();
                        var Time = itemEvent.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.SelectMany(f => f.EventIntendedForAtdCheckStudents.Select(h => h.Time).ToList()).ToList()).FirstOrDefault();
                        result.Add(new GetUnsubmittedAttendanceEventResult
                        {
                            idEvent = itemEvent.Id,
                            eventName = itemEvent.Name,
                            StartDate = itemDateEvent.StartDate,
                            EndDate = itemDateEvent.EndDate,
                            AttendanceCheckName = CheckName,
                            AttendanceTime = Time,
                        });
                    }
                }
            }
            return Request.CreateApiResult2(result as object);
        }
    }
}
