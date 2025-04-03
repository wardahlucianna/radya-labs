using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Eval.Forked;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnLongRun.Services
{
    public class EventSchoolService : IEventSchoolService
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<EventSchoolService> _logger;
        private readonly Dictionary<string, string> _dictSchool;
        //private readonly Dictionary<string, string> _dictAcademicYearBySchool;

        public EventSchoolService(ISchedulingDbContext dbContext, IMachineDateTime dateTime,
            ILogger<EventSchoolService> logger)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _logger = logger;
            _dictSchool = new Dictionary<string, string>();
            //_dictAcademicYearBySchool = new Dictionary<string, string>();
        }

        public async Task<string> GetEventSchool(string idSchool, bool halfDay, CancellationToken cancellationToken)
        {
            if (_dictSchool.ContainsKey(idSchool))
                return _dictSchool[idSchool];

            var result = await _dbContext.Entity<MsSchool>().Where(e => e.Id == idSchool)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return null;

            var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                .Include(e => e.Grade).ThenInclude(e => e.Level)
                .Where(e => e.Grade.Level.AcademicYear.IdSchool == idSchool
                        && e.StartDate.Date <= _dateTime.ServerTime.Date
                        && e.EndDate.Date >= _dateTime.ServerTime.Date)
                .Select(e => e.Grade.Level.IdAcademicYear)
                .FirstOrDefaultAsync(cancellationToken);

            if (idAcademicYear == null)
                return null;

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                     .Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear)
                     .ToListAsync(cancellationToken);

            var startdateEvent = _dateTime.ServerTime.Date;

            if (halfDay == false)
                startdateEvent = listPeriod.OrderBy(e=>e.StartDate).Select(e=>e.StartDate).FirstOrDefault();

            #region active scheduleLesson
            var listEventNotActive = await _dbContext.Entity<TrEventDetail>()
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonalStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForLevelStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForGradeStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventSchedules)
                .IgnoreQueryFilters()
                .Where(e => e.Event.IdAcademicYear == idAcademicYear
                        && (e.StartDate.Date >= startdateEvent || e.EndDate.Date >= startdateEvent)
                        && e.Event.EventIntendedFor.Any(e => e.IntendedFor == "STUDENT")
                        && e.Event.StatusEvent == "Approved"
                        && !e.Event.IsActive
                        && e.Event.EventIntendedFor
                            .Any(e => e.EventIntendedForAttendanceStudents
                                .Any(e => e.Type == EventIntendedForAttendanceStudent.NoAtdClass))
                        )
                .ToListAsync(cancellationToken);

            var listIdScheduleLessonNotActive = listEventNotActive.SelectMany(e => e.Event.EventSchedules.Select(f => f.IdScheduleLesson)).ToList();

            var listScheduleLessonNotActive = await _dbContext.Entity<MsScheduleLesson>()
                                           .Include(e => e.Lesson)
                                           .IgnoreQueryFilters()
                                           .Where(x => listIdScheduleLessonNotActive.Contains(x.Id) 
                                                    && x.IsDeleteFromEvent
                                                    && !x.IsActive)
                                           .ToListAsync(cancellationToken);

            listScheduleLessonNotActive.ForEach(e => {
                e.IsActive = true;
                e.IsDeleteFromEvent = false;
            });

            _dbContext.Entity<MsScheduleLesson>().UpdateRange(listScheduleLessonNotActive);

            await _dbContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Add/check
            var listEvent = await _dbContext.Entity<TrEventDetail>()
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonalStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForLevelStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForGradeStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents)
                .Where(e => e.Event.IdAcademicYear == idAcademicYear
                        && (e.StartDate.Date >= startdateEvent || e.EndDate.Date >= startdateEvent)
                        && e.Event.EventIntendedFor.Any(e => e.IntendedFor == "STUDENT")
                        && e.Event.StatusEvent == "Approved"
                        && e.Event.EventIntendedFor
                            .Any(e => e.EventIntendedForAttendanceStudents
                                .Any(e => e.Type == EventIntendedForAttendanceStudent.All 
                                    || e.Type == EventIntendedForAttendanceStudent.NoAtdClass
                                    || e.Type == EventIntendedForAttendanceStudent.Excuse
                                    ))
                        )
                .ToListAsync(cancellationToken);

            List<DateTime> dates = new List<DateTime>();
            foreach(var item in listEvent)
            {
                for(var i=item.StartDate.Date; i <= item.EndDate.Date; i=i.AddDays(1))
                {
                    if (!dates.Contains(i))
                        dates.Add(i);
                }
            }

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                               .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
                               .GroupBy(e => new GetHomeroom
                               {
                                   IdLesson = e.IdLesson,
                                   IdStudent = e.HomeroomStudent.IdStudent,
                                   IdHomeroomStudent = e.HomeroomStudent.Id,
                                   Homeroom = new ItemValueVm
                                   {
                                       Id = e.HomeroomStudent.IdHomeroom,
                                   },
                                   ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                   Grade = new CodeWithIdVm
                                   {
                                       Id = e.HomeroomStudent.Homeroom.IdGrade,
                                       Code = e.HomeroomStudent.Homeroom.Grade.Code,
                                       Description = e.HomeroomStudent.Homeroom.Grade.Description
                                   },
                                   Level = new CodeWithIdVm
                                   {
                                       Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                                       Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                                       Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                                   },
                                   Semester = e.HomeroomStudent.Homeroom.Semester,
                                   IdHomeroomStudentEnrollment = e.Id
                               })
                               .Select(e => new GetHomeroom
                               {
                                   IdLesson = e.Key.IdLesson,
                                   IdStudent = e.Key.IdStudent,
                                   IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                   Homeroom = new ItemValueVm
                                   {
                                       Id = e.Key.Homeroom.Id,
                                   },
                                   ClassroomCode = e.Key.ClassroomCode,
                                   Grade = new CodeWithIdVm
                                   {
                                       Id = e.Key.Grade.Id,
                                       Code = e.Key.Grade.Code,
                                       Description = e.Key.Grade.Description
                                   },
                                   Level = new CodeWithIdVm
                                   {
                                       Id = e.Key.Level.Id,
                                       Code = e.Key.Level.Code,
                                       Description = e.Key.Level.Description
                                   },
                                   Semester = e.Key.Semester,
                                   IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                   IsFromMaster = true,
                                   IsDelete = false,
                               })
                               .ToListAsync(cancellationToken);

            listHomeroomStudentEnrollment.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min());

            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                           .Include(e => e.SubjectNew)
                           .Include(e => e.LessonNew)
                           .Where(x => x.LessonOld.IdAcademicYear == idAcademicYear)
                           .Select(e => new GetHomeroom
                           {
                               IdLesson = e.IdLessonNew,
                               IdStudent = e.HomeroomStudent.IdStudent,
                               IdHomeroomStudent = e.IdHomeroomStudent,
                               Homeroom = new ItemValueVm
                               {
                                   Id = e.HomeroomStudent.Homeroom.Id,
                               },
                               ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                               Grade = new CodeWithIdVm
                               {
                                   Id = e.HomeroomStudent.Homeroom.Grade.Id,
                                   Code = e.HomeroomStudent.Homeroom.Grade.Code,
                                   Description = e.HomeroomStudent.Homeroom.Grade.Description
                               },
                               Level = new CodeWithIdVm
                               {
                                   Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                                   Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                                   Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                               },
                               Semester = e.HomeroomStudent.Homeroom.Semester,
                               IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                               IsFromMaster = false,
                               EffectiveDate = e.StartDate,
                               IsDelete = e.IsDelete
                           })
                           .ToListAsync(cancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                    .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                    .ToList();

            var listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                           .Include(e => e.Lesson)
                           .IgnoreQueryFilters()
                           .Where(x => x.IdAcademicYear == idAcademicYear && x.IsGenerated && dates.Contains(x.ScheduleDate))
                           .ToListAsync(cancellationToken);

            var maxAttendanceEndDatePeriod = listPeriod.Select(e => e.AttendanceEndDate).Max();

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                               .Where(e => e.IdAcademicYear == idAcademicYear && e.ActiveStatus)
                               .Select(e => new
                               {
                                   e.IdStudent,
                                   e.StartDate,
                                   endDate = e.EndDate == null
                                           ? maxAttendanceEndDatePeriod
                                           : Convert.ToDateTime(e.EndDate),
                               })
                               .ToListAsync(cancellationToken);

            List<GetEventScheduleStudent> listEventScheduleStudent = new List<GetEventScheduleStudent>();
            foreach (var item in listEvent)
            {
                var listIntendedFor = item.Event.EventIntendedFor.Where(e => e.IntendedFor == "STUDENT").ToList();
                var typeAttendanceStudent = listIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.Select(e => e.Type)).FirstOrDefault();

                List<GetEventStudent> listEventStudent = new List<GetEventStudent>();
                foreach (var itemOption in listIntendedFor)
                {
                    var option = itemOption.Option;

                    if (option.ToLower() == "all")
                    {
                        var listStudentAll = listStudentEnrollmentUnion
                                                .Select(e => new GetEventStudent
                                                {
                                                    IdEvent = item.IdEvent,
                                                    IdStudent = e.IdStudent
                                                }).ToList();
                        listEventStudent.AddRange(listStudentAll);
                    }
                    else if (option.ToLower() == "level")
                    {
                        var listeventIntendedForStudentByLevel = itemOption.EventIntendedForLevelStudents.Select(r => r.IdLevel).ToList();

                        var listStudentByLevel = listStudentEnrollmentUnion
                                        .Where(e => listeventIntendedForStudentByLevel.Contains(e.Level.Id))
                                        .Select(e => new GetEventStudent
                                        {
                                            IdEvent = item.IdEvent,
                                            IdStudent = e.IdStudent
                                        })
                                        .ToList();

                        listEventStudent.AddRange(listStudentByLevel);
                    }
                    else if (option.ToLower() == "grade")
                    {
                        var listeventIntendedForStudentByGrade = itemOption.EventIntendedForGradeStudents.Select(r => r.IdHomeroom).ToList();

                        var listStudentByGrade = listStudentEnrollmentUnion
                                       .Where(e => listeventIntendedForStudentByGrade.Contains(e.Homeroom.Id))
                                       .Select(e => new GetEventStudent
                                       {
                                           IdEvent = item.IdEvent,
                                           IdStudent = e.IdStudent
                                       })
                                       .ToList();

                        listEventStudent.AddRange(listStudentByGrade);
                    }
                    else if (option.ToLower() == "personal")
                    {
                        var listStudentByPersonal = itemOption.EventIntendedForPersonalStudents.Select(e => new GetEventStudent
                        {
                            IdEvent = item.IdEvent,
                            IdStudent = e.IdStudent
                        })
                            .ToList();
                        listEventStudent.AddRange(listStudentByPersonal);
                    }
                }

                var StartTime = item.StartDate.TimeOfDay;
                var EndTime = item.EndDate.TimeOfDay;
                var listIdStudentEvnt = listEventStudent.Select(e => e.IdStudent).ToList();

                var listScheduleLessonByEvent = listScheduleLesson
                   .Where(e => item.StartDate.Date <= e.ScheduleDate && item.EndDate.Date >= e.ScheduleDate.Date)
                   .Where(e => StartTime < e.StartTime || StartTime < e.EndTime)
                   .Where(e => EndTime > e.StartTime || EndTime > e.EndTime)
                   .ToList();

                //if (halfDay)
                //    listScheduleLessonByEvent = listScheduleLessonByEvent.Where(e=>e.ScheduleDate.Date==_dateTime.ServerTime.Date).ToList();
                

                foreach (var itemScheduleLesson in listScheduleLessonByEvent)
                {
                    _logger.LogInformation($"AY {idAcademicYear} - Count Event {listEvent.IndexOf(item) + 1}/{listEvent.Count()} - Count Schedule Lesson {listScheduleLessonByEvent.IndexOf(itemScheduleLesson) + 1}/{listScheduleLessonByEvent.Count()}");

                    var semester = listPeriod.Where(e => e.IdGrade == itemScheduleLesson.IdGrade
                                                    && e.StartDate.Date <= itemScheduleLesson.ScheduleDate.Date 
                                                    && e.EndDate.Date >= itemScheduleLesson.ScheduleDate.Date)
                                        .Select(e=>e.Semester)
                                        .FirstOrDefault();

                    if (semester != itemScheduleLesson.Lesson.Semester) 
                        continue;

                    var listStatusStudentByDate = listStudentStatus
                                        .Where(e => e.StartDate.Date <= itemScheduleLesson.ScheduleDate.Date
                                                    && e.endDate.Date >= itemScheduleLesson.ScheduleDate.Date)
                                        .Select(e => e.IdStudent).ToList();

                    var listStudentEnrollmentMoving = GetMovingStudent(listStudentEnrollmentUnion, itemScheduleLesson.ScheduleDate, itemScheduleLesson.Lesson.Semester.ToString(), itemScheduleLesson.IdLesson);

                    var _listIdStudentEvnt = listIdStudentEvnt.Where(e => listStatusStudentByDate.Contains(e)).Distinct().ToList();
                    var studentEnrollmentMoving = listStudentEnrollmentMoving
                                                  .Where(e => _listIdStudentEvnt.Contains(e.IdStudent))
                                                  .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                  .ToList();

                    if (studentEnrollmentMoving.Any())
                    {
                        var newGetEventScheduleStudent = new GetEventScheduleStudent
                        {
                            IdEvent = item.IdEvent,
                            IdScheduleLesson = itemScheduleLesson.Id,
                        };

                        listEventScheduleStudent.Add(newGetEventScheduleStudent);
                    }
                }
            }

            var newTrEventSchedule = listEventScheduleStudent
                       .GroupBy(e => new
                       {
                           e.IdEvent,
                           e.IdScheduleLesson,
                       })
                       .Select(e => new TrEventSchedule
                       {
                           Id = Guid.NewGuid().ToString(),
                           IdEvent = e.Key.IdEvent,
                           IdScheduleLesson = e.Key.IdScheduleLesson,
                       })
                       .ToList();

            var listIdEvent = newTrEventSchedule.Select(e => e.IdEvent).Distinct().ToList();

            var listEventSchedule = await _dbContext.Entity<TrEventSchedule>()
                                        .IgnoreQueryFilters()
                                      .Where(e => listIdEvent.Contains(e.IdEvent))
                                      .ToListAsync(cancellationToken);

            foreach (var idEvent in listIdEvent)
            {
                var listEventScheduleByEvent = listEventSchedule.Where(e => e.IdEvent == idEvent).ToList();
                var listNewTrEventScheduleByEvent = newTrEventSchedule.Where(e => e.IdEvent == idEvent).ToList();

                var listIdScheduleLesson = listEventScheduleByEvent.Select(e => e.IdScheduleLesson).ToList();
                var listNewIdScheduleLesson = listNewTrEventScheduleByEvent.Select(e => e.IdScheduleLesson).ToList();

                var listNewTrEventScheduleByEventRemove = listEventScheduleByEvent
                                    .Where(e => !listNewIdScheduleLesson.Contains(e.IdScheduleLesson))
                                    .ToList();

                listNewTrEventScheduleByEventRemove.ForEach(e => 
                {
                    e.IsActive = false;
                    e.IsSyncAttendance = false;
                    e.DateSyncAttendance = null;
                });

                var listNewTrEventScheduleByEventUpdate = listEventScheduleByEvent
                                    .Where(e => listNewIdScheduleLesson.Contains(e.IdScheduleLesson) && !e.IsActive)
                                    .ToList();

                var listNewTrEventScheduleByEventUpdateGroup = listNewTrEventScheduleByEventUpdate
                                                                .GroupBy(e => new
                                                                {
                                                                    e.IdScheduleLesson
                                                                });

                foreach(var itemGroup in listNewTrEventScheduleByEventUpdateGroup)
                {
                    var _listEventScheduleGroup = itemGroup.OrderBy(e=>e.DateIn).ToList();
                    var last = _listEventScheduleGroup.Last();

                    if (!last.IsActive)
                    {
                        _listEventScheduleGroup.Last().IsActive = true;
                        _dbContext.Entity<TrEventSchedule>().Update(last);
                    }

                    var _listEventScheduleGroupNoActive = _listEventScheduleGroup.Where(e => e.Id != last.Id && e.IsActive).ToList();
                    _listEventScheduleGroupNoActive.ForEach(e=> e.IsActive = false);
                    _dbContext.Entity<TrEventSchedule>().UpdateRange(_listEventScheduleGroupNoActive);
                }

                var listNewTrEventScheduleByEventAdd = listNewTrEventScheduleByEvent
                                    .Where(e => !listIdScheduleLesson.Contains(e.IdScheduleLesson))
                                    .ToList();

                _dbContext.Entity<TrEventSchedule>().UpdateRange(listNewTrEventScheduleByEventRemove);
                _dbContext.Entity<TrEventSchedule>().AddRange(listNewTrEventScheduleByEventAdd);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region mengaktifkan schedule Lesson
            var listEventScheduleNotActive = await _dbContext.Entity<TrEventSchedule>()
               .IgnoreQueryFilters()
               .Where(e => e.Event.IdAcademicYear == idAcademicYear 
                        && listIdEvent.Contains(e.IdEvent) 
                        && !e.IsActive 
                        && !e.IsSyncAttendance
                        && e.Event.EventIntendedFor.Any(f => f.EventIntendedForAttendanceStudents.Any(g => g.Type == EventIntendedForAttendanceStudent.NoAtdClass)))
               .ToListAsync(cancellationToken);

            var listIdScheduleNotActive = listEventScheduleNotActive.Select(e => e.IdScheduleLesson).Distinct().ToList();

            listScheduleLessonNotActive = await _dbContext.Entity<MsScheduleLesson>()
                          .Include(e => e.Lesson)
                          .IgnoreQueryFilters()
                          .Where(x => x.IdAcademicYear == idAcademicYear && listIdScheduleNotActive.Contains(x.Id) && x.IsGenerated)
                          .ToListAsync(cancellationToken);

            listScheduleLessonNotActive.ForEach(e =>
            {
                e.IsActive = true;
                e.IsDeleteFromEvent = false;
            });
            _dbContext.Entity<MsScheduleLesson>().UpdateRange(listScheduleLessonNotActive);

            var updateEventScheduleNotActive = listEventScheduleNotActive.Where(e => listIdScheduleLessonNotActive.Contains(e.IdScheduleLesson)).ToList();
            listEventScheduleNotActive.ForEach(e =>
            {
                e.IsSyncAttendance = true;
                e.DateSyncAttendance = _dateTime.ServerTime;
            });
            _dbContext.Entity<TrEventSchedule>().UpdateRange(updateEventScheduleNotActive);

            await _dbContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region mengnonaktifkan schedule Lesson
            var listEventScheduleActive = await _dbContext.Entity<TrEventSchedule>()
               .Where(e => e.Event.IdAcademicYear == idAcademicYear
                    && listIdEvent.Contains(e.IdEvent)
                    && !e.IsSyncAttendance
                    && e.Event.EventIntendedFor.Any(f => f.EventIntendedForAttendanceStudents.Any(g => g.Type == EventIntendedForAttendanceStudent.NoAtdClass)))
               .ToListAsync(cancellationToken);

            var listIdScheduleLessonActive = listEventScheduleActive.Select(e => e.IdScheduleLesson).ToList();

            var listScheduleLessonActive = await _dbContext.Entity<MsScheduleLesson>()
                          .Include(e => e.Lesson)
                          .Where(x => x.IdAcademicYear == idAcademicYear && listIdScheduleLessonActive.Contains(x.Id) && x.IsGenerated)
                          .ToListAsync(cancellationToken);

            listScheduleLessonActive.ForEach(e =>
            {
                e.IsActive = false;
                e.IsDeleteFromEvent = true;
            });
            _dbContext.Entity<MsScheduleLesson>().UpdateRange(listScheduleLessonActive);

            var updateEventScheduleActive = listEventScheduleActive.Where(e => listIdScheduleLessonActive.Contains(e.IdScheduleLesson)).ToList();
            updateEventScheduleActive.ForEach(e =>
            {
                e.IsSyncAttendance = true;
                e.DateSyncAttendance = _dateTime.ServerTime;
            });
            _dbContext.Entity<TrEventSchedule>().UpdateRange(updateEventScheduleActive);

            await _dbContext.SaveChangesAsync(cancellationToken);
            #endregion

            _dictSchool.Add(idSchool, result.Description);

            return _dictSchool[idSchool];
        }

        public static List<GetHomeroom> GetMovingStudent(List<GetHomeroom> listStudentEnrollmentUnion, DateTime scheduleDate, string semester, string idLesson)
        {
            var listIdHomeroomStudentEnrollment = listStudentEnrollmentUnion.Where(e => e.IdLesson == idLesson).Select(e => e.IdHomeroomStudentEnrollment)
                                                    .Distinct().ToList();

            listStudentEnrollmentUnion = listStudentEnrollmentUnion.Where(e => listIdHomeroomStudentEnrollment.Contains(e.IdHomeroomStudentEnrollment)).ToList();

            var listStudentEnrollmentByDate = listStudentEnrollmentUnion
                                                .Where(e => e.EffectiveDate.Date <= scheduleDate.Date && e.Semester.ToString() == semester)
                                                .ToList();

            listIdHomeroomStudentEnrollment = listStudentEnrollmentByDate.Select(e => e.IdHomeroomStudentEnrollment).Distinct().ToList();

            var listStudentEnrollmentNew = new List<GetHomeroom>();
            foreach (var idHomeroomStudentEnrollment in listIdHomeroomStudentEnrollment)
            {
                var studentEnrollment = listStudentEnrollmentByDate
                                        .Where(e => e.IdHomeroomStudentEnrollment == idHomeroomStudentEnrollment)
                                        .LastOrDefault();

                if (studentEnrollment.IdLesson == idLesson && !studentEnrollment.IsDelete)
                {
                    listStudentEnrollmentNew.Add(studentEnrollment);
                }

            }

            return listStudentEnrollmentNew;
        }


    }

    public class GetEventStudent
    {
        public string IdEvent { get; set; }
        public string IdStudent { get; set; }
    }

    public class GetEventScheduleStudent
    {
        public string IdEvent { get; set; }
        public string IdStudent { get; set; }
        public string IdScheduleLesson { get; set; }
    }

}
