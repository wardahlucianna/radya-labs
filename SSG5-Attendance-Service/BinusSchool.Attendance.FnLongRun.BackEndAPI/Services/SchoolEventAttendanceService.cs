using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.XWPF.UserModel;

namespace BinusSchool.Attendance.FnLongRun.Service
{
    public class SchoolEventAttendanceService 
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<SchoolEventAttendanceService> _logger;
        private readonly Dictionary<string, string> _dictSchool;
        private readonly IEventSchool _apiEventSchool;
        public SchoolEventAttendanceService(IAttendanceDbContext dbContext, IMachineDateTime dateTime,
            ILogger<SchoolEventAttendanceService> logger, IEventSchool apiEventSchool)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _logger = logger;
            _apiEventSchool = apiEventSchool;
            _dictSchool = new Dictionary<string, string>();
        }

        public async Task RunAsync(string idSchool, CancellationToken cancellationToken)
        {
            try
            {
                await GetSchoolEventAttendance(idSchool, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurs");
            }
        }

        public async Task<string> GetSchoolEventAttendance(string idSchool, CancellationToken cancellationToken)
        {
            if (_dictSchool.ContainsKey(idSchool))
                return _dictSchool[idSchool];

            var result = await _dbContext.Entity<MsSchool>().Where(e => e.Id == idSchool)
                .Select(e => e.Description)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return null;

            _logger.LogInformation("School Event Attendancecronjob for school {Name} has started..", result);

            _dictSchool.Add(idSchool, result);

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

            #region remove Attendance data double
            var _listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                               .Where(e => e.ScheduleLesson.IdAcademicYear==idAcademicYear && e.PositionIn == "EventSys")
                                .OrderByDescending(e => e.DateIn)
                                .ToListAsync(cancellationToken);

            var listAttendanceGroup = _listAttendanceEntry.GroupBy(e => new
            {
                e.IdScheduleLesson,
                e.IdHomeroomStudent
            }).Where(e=>e.Count()>1);

            foreach (var item in listAttendanceGroup)
            {
                var listAttendance = item.OrderByDescending(e => e.DateIn).ToList();

                if (listAttendance.Count() > 1)
                {
                    var idAttendance = listAttendance.Select(e => e.IdAttendanceEntry).FirstOrDefault();
                    var listRemoveAttendance = listAttendance.Where(e => e.IdAttendanceEntry != idAttendance).ToList();
                    listRemoveAttendance.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(listRemoveAttendance);
                }
            }
            await _dbContext.SaveChangesAsync();
            #endregion


            #region
            var apiGetEventSchedule = await _apiEventSchool.GetEventSchedule(new GetEventScheduleRequest
            {
                IdAcademicYear = idAcademicYear,
            });

            var listEventSchedule = apiGetEventSchedule.IsSuccess
                                        ? apiGetEventSchedule.Payload.ToList()
                                        : null;
            
            if (!listEventSchedule.Any())
                return _dictSchool[idSchool];

            var listIdEvent = listEventSchedule.Select(e => e.IdEvent).Distinct().ToList();
            var listIdScheduleLesson = listEventSchedule.Select(e => e.IdScheduleLesson).Distinct().ToList();

            var listEvent = await _dbContext.Entity<TrEventDetail>()
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonalStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForLevelStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForGradeStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents)
                .Include(e => e.Event).ThenInclude(e => e.EventType)
                .Where(e => listIdEvent.Contains(e.IdEvent))
                .ToListAsync(cancellationToken);

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
                           .Where(x => listIdScheduleLesson.Contains(x.Id))
                           .ToListAsync(cancellationToken);

            var listIdLesson = listScheduleLesson.Select(e => e.IdLesson).Distinct().ToList();
            var listIdDay = listScheduleLesson.Select(e => e.IdDay).Distinct().ToList();

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




            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                               .Where(e => listIdScheduleLesson.Contains(e.IdScheduleLesson))
                               .IgnoreQueryFilters()
                               .ToListAsync(cancellationToken);

            var listSchedule = await _dbContext.Entity<MsSchedule>()
                               .Where(e => listIdLesson.Contains(e.IdLesson)
                                           && listIdDay.Contains(e.IdDay))
                               .Select(e => new
                               {
                                   e.IdUser,
                                   e.IdLesson,
                                   e.IdSession,
                                   e.IdDay,
                                   e.IdWeek
                               })
                               .ToListAsync(cancellationToken);

            var listAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                            .Include(e=>e.Attendance)
                                            .Include(e=>e.MappingAttendance)
                                            .Where(e => e.Attendance.IdAcademicYear==idAcademicYear
                                                        && (e.Attendance.Description == "Excused Absence" || e.Attendance.Description == "Present")
                                                        && e.MappingAttendance.Level.IdAcademicYear==idAcademicYear)
                                           .ToListAsync(cancellationToken);

            List<GetEventScheduleStudent> listEventScheduleStudent = new List<GetEventScheduleStudent>();
            foreach (var itemEvent in listEvent)
            {
                var listIntendedFor = itemEvent.Event.EventIntendedFor.Where(e => e.IntendedFor == "STUDENT").ToList();
                List<GetEventStudentHomeroom> listEventStudentHomeroom = new List<GetEventStudentHomeroom>();
                foreach (var itemOption in listIntendedFor)
                {
                    var option = itemOption.Option;

                    if (option.ToLower() == "all")
                    {
                        var listStudentAll = listStudentEnrollmentUnion
                                                .Select(e => new GetEventStudentHomeroom
                                                {
                                                    IdEvent = itemEvent.IdEvent,
                                                    IdHomeroomStudent = e.IdHomeroomStudent,
                                                    Semester = e.Semester
                                                }).ToList();
                        listEventStudentHomeroom.AddRange(listStudentAll);
                    }
                    else if (option.ToLower() == "level")
                    {
                        var listeventIntendedForStudentByLevel = itemOption.EventIntendedForLevelStudents.Select(r => r.IdLevel).ToList();

                        var listStudentByLevel = listStudentEnrollmentUnion
                                        .Where(e => listeventIntendedForStudentByLevel.Contains(e.Level.Id))
                                        .Select(e => new GetEventStudentHomeroom
                                        {
                                            IdEvent = itemEvent.IdEvent,
                                            IdHomeroomStudent = e.IdHomeroomStudent,
                                            Semester = e.Semester
                                        })
                                        .ToList();

                        listEventStudentHomeroom.AddRange(listStudentByLevel);
                    }
                    else if (option.ToLower() == "grade")
                    {
                        var listeventIntendedForStudentByGrade = itemOption.EventIntendedForGradeStudents.Select(r => r.IdHomeroom).ToList();

                        var listStudentByGrade = listStudentEnrollmentUnion
                                       .Where(e => listeventIntendedForStudentByGrade.Contains(e.Homeroom.Id))
                                       .Select(e => new GetEventStudentHomeroom
                                       {
                                           IdEvent = itemEvent.IdEvent,
                                           IdHomeroomStudent = e.IdHomeroomStudent,
                                           Semester = e.Semester
                                       })
                                       .ToList();

                        listEventStudentHomeroom.AddRange(listStudentByGrade);
                    }
                    else if (option.ToLower() == "personal")
                    {
                        var listIdStudentByPersonal = itemOption.EventIntendedForPersonalStudents
                                                        .Select(e=>e.IdStudent)
                                                        .ToList();

                        var listStudentByPersonal = listStudentEnrollmentUnion
                                      .Where(e => listIdStudentByPersonal.Contains(e.IdStudent))
                                      .Select(e => new GetEventStudentHomeroom
                                      {
                                          IdEvent = itemEvent.IdEvent,
                                          IdHomeroomStudent = e.IdHomeroomStudent,
                                          Semester = e.Semester
                                      })
                                      .ToList();

                       
                        listEventStudentHomeroom.AddRange(listStudentByPersonal);
                    }
                }


                var typeAttendance = itemEvent.Event.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.Select(e => e.Type)).FirstOrDefault();

                var listEventScheduleByEvent = listEventSchedule.Where(e => e.IdEvent == itemEvent.IdEvent).ToList();
                foreach(var itemEventSchedule in listEventScheduleByEvent)
                {
                    _logger.LogInformation($"AY {idAcademicYear} - Count Event {listEvent.IndexOf(itemEvent) + 1}/{listEvent.Count()} - Count Event Schedule {listEventScheduleByEvent.IndexOf(itemEventSchedule) + 1}/{listEventScheduleByEvent.Count()}");

                    var scheduleLessonByEvent = listScheduleLesson.Where(e => e.Id== itemEventSchedule.IdScheduleLesson).FirstOrDefault();

                    var semester = listPeriod.Where(e => e.IdGrade == scheduleLessonByEvent.IdGrade
                                                    && e.StartDate.Date <= scheduleLessonByEvent.ScheduleDate.Date
                                                    && e.EndDate.Date >= scheduleLessonByEvent.ScheduleDate.Date)
                                        .Select(e => e.Semester)
                                        .FirstOrDefault();

                    if (semester != scheduleLessonByEvent.Lesson.Semester)
                        continue;

                    var listStatusStudentByDate = listStudentStatus
                                            .Where(e => e.StartDate.Date <= scheduleLessonByEvent.ScheduleDate.Date
                                                        && e.endDate.Date >= scheduleLessonByEvent.ScheduleDate.Date)
                                            .Select(e => e.IdStudent).ToList();

                    var listStudentEnrollmentMoving = GetMovingStudent(listStudentEnrollmentUnion, scheduleLessonByEvent.ScheduleDate, scheduleLessonByEvent.Lesson.Semester.ToString(), scheduleLessonByEvent.IdLesson);

                    var listIdStudentEvnt = listEventStudentHomeroom.Select(e => e.IdHomeroomStudent).ToList();

                    var listHomeroomStudentMoving = listStudentEnrollmentMoving
                                                  .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && listIdStudentEvnt.Contains(e.IdHomeroomStudent))
                                                  .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                  .ToList();

                    var listHomeroomStudent = listHomeroomStudentMoving
                                                .GroupBy(e => new
                                                {
                                                    e.IdHomeroomStudent,
                                                    IdLevel = e.Level.Id
                                                })
                                                .Select(e=>e.Key).ToList();

                    if (listHomeroomStudent.Any())
                    {
                         foreach (var itemStudent in listHomeroomStudent)
                         {
                            var listAttendanceByIdHomeroomStudent = listAttendanceEntry
                                                                       .Where(e => e.IdHomeroomStudent == itemStudent.IdHomeroomStudent
                                                                           && e.IdScheduleLesson == itemEventSchedule.IdScheduleLesson
                                                                           && !(e.PositionIn== "EventSys" && !e.IsActive))
                                                                       .OrderByDescending(e => e.DateIn)
                                                                       .ToList();

                            var listAttendanceFirst = listAttendanceByIdHomeroomStudent.Where(e=>e.IsActive).FirstOrDefault();
                            var indexFirst = listAttendanceByIdHomeroomStudent.IndexOf(listAttendanceFirst);

                            TrAttendanceEntryV2 listAttendanceLast = null;
                            if (indexFirst >=0 && listAttendanceByIdHomeroomStudent.Count() > 1)
                            {
                                var index = indexFirst + 1;

                                if(index< listAttendanceByIdHomeroomStudent.Count())
                                    listAttendanceLast = listAttendanceByIdHomeroomStudent[index];
                            }

                            var idAttendanceMappingAttendance = "";
                            if (typeAttendance == EventIntendedForAttendanceStudent.All)
                            {
                                idAttendanceMappingAttendance = listAttendanceMappingAttendance
                                                                .Where(e => e.Attendance.Description == "Present"
                                                                    && e.MappingAttendance.IdLevel == itemStudent.IdLevel)
                                                                .Select(e=>e.Id)
                                                                .FirstOrDefault();
                            }
                            else if (typeAttendance == EventIntendedForAttendanceStudent.Excuse)
                            {
                                idAttendanceMappingAttendance = listAttendanceMappingAttendance
                                                                .Where(e => e.Attendance.Description == "Excused Absence"
                                                                    && e.MappingAttendance.IdLevel == itemStudent.IdLevel)
                                                                .Select(e => e.Id)
                                                                .FirstOrDefault();
                            }

                            if (string.IsNullOrEmpty(idAttendanceMappingAttendance))
                                continue;

                            if (itemEventSchedule.IsActive)
                            {
                                if (listAttendanceFirst != null)
                                {
                                    if (listAttendanceFirst.IsFromAttendanceAdministration)
                                        continue;
                                    else
                                    {
                                        if (listAttendanceFirst.PositionIn == "EventSys")
                                        {
                                            listAttendanceFirst.IdAttendanceMappingAttendance = idAttendanceMappingAttendance;
                                            listAttendanceFirst.Notes = itemEvent.Event.EventType.Description;
                                            _dbContext.Entity<TrAttendanceEntryV2>().Update(listAttendanceFirst);
                                        }
                                        else
                                        {
                                            listAttendanceFirst.IsActive = false;
                                            _dbContext.Entity<TrAttendanceEntryV2>().Update(listAttendanceFirst);

                                            //create attendance entry
                                            var newAttendanceEntry = new TrAttendanceEntryV2
                                            {
                                                IdAttendanceEntry = Guid.NewGuid().ToString(),
                                                IdScheduleLesson = itemEventSchedule.IdScheduleLesson,
                                                IdAttendanceMappingAttendance = idAttendanceMappingAttendance,
                                                LateTime = default,
                                                FileEvidence = null,
                                                Notes = itemEvent.Event.EventType.Description,
                                                Status = AttendanceEntryStatus.Submitted,
                                                IsFromAttendanceAdministration = false,
                                                PositionIn = "EventSys",
                                                IdBinusian = listAttendanceFirst.IdBinusian,
                                                IdHomeroomStudent = listAttendanceFirst.IdHomeroomStudent
                                            };

                                            _dbContext.Entity<TrAttendanceEntryV2>().Add(newAttendanceEntry);
                                        }

                                    }
                                }
                                else
                                {
                                    var idTeacher = listSchedule
                                                .Where(e => e.IdLesson == scheduleLessonByEvent.IdLesson
                                                       && e.IdDay == scheduleLessonByEvent.IdDay
                                                       && e.IdSession == scheduleLessonByEvent.IdSession
                                                       && e.IdWeek == scheduleLessonByEvent.IdWeek
                                                        )
                                                .Select(e => e.IdUser)
                                                .FirstOrDefault();

                                    if (idTeacher == null)
                                        continue;

                                    var newAttendanceEntry = new TrAttendanceEntryV2
                                    {
                                        IdAttendanceEntry = Guid.NewGuid().ToString(),
                                        IdScheduleLesson = itemEventSchedule.IdScheduleLesson,
                                        IdAttendanceMappingAttendance = idAttendanceMappingAttendance,
                                        LateTime = default,
                                        FileEvidence = null,
                                        Notes = itemEvent.Event.EventType.Description,
                                        Status = AttendanceEntryStatus.Submitted,
                                        IsFromAttendanceAdministration = false,
                                        PositionIn = "EventSys",
                                        IdBinusian = idTeacher,
                                        IdHomeroomStudent = itemStudent.IdHomeroomStudent
                                    };

                                    _dbContext.Entity<TrAttendanceEntryV2>().Add(newAttendanceEntry);
                                }
                            }
                            else
                            {
                                if (listAttendanceFirst != null)
                                {
                                    if (listAttendanceFirst.IsFromAttendanceAdministration)
                                        continue;
                                    else
                                    {
                                        if (listAttendanceFirst.PositionIn == "EventSys")
                                        {
                                            listAttendanceFirst.IsActive = false;
                                            _dbContext.Entity<TrAttendanceEntryV2>().Update(listAttendanceFirst);

                                            if (listAttendanceLast != null)
                                            {
                                                listAttendanceLast.IsActive = true;
                                                _dbContext.Entity<TrAttendanceEntryV2>().Update(listAttendanceLast);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    var paramUpdateStatusSyncEventSchedule = new UpdateStatusSyncEventScheduleRequest
                                                                {
                                                                    IdEventSchedule = itemEventSchedule.IdEventSchedule
                                                                };

                    var apiUpdateStatusSyncEventSchedule = await _apiEventSchool.UpdateStatusSyncEventSchedule(paramUpdateStatusSyncEventSchedule);
                }
            }

            #endregion
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

    public class GetEventStudentHomeroom
    {
        public string IdEvent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public int Semester {  get; set; }
    }

    public class GetEventScheduleStudent
    {
        public string IdEvent { get; set; }
        public string IdStudent { get; set; }
        public string IdScheduleLesson { get; set; }
    }
}
