using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel.Helpers;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetSessionAttendanceV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetimeNow;

        public GetSessionAttendanceV2Handler(IAttendanceDbContext dbContext, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceV2Request>();

            var scheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.IdBinusianSubtitute == param.IdUser && x.ScheduleDate == param.Date)
                    .ToListAsync(CancellationToken);

            var scheduleRealizationChanged = await _dbContext.Entity<TrScheduleRealization2>()
                .Where(x => x.IdBinusian == param.IdUser && x.Status == "Subtituted" && x.ScheduleDate == param.Date && x.IdBinusian != x.IdBinusianSubtitute)
                .ToListAsync(CancellationToken);

            var listIdLessonRealization = scheduleRealization.Select(x => x.IdLesson).ToList();

            var listIdLessonByLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                                 .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                                                 .Where(e => e.IdUser == param.IdUser
                                                         && e.Lesson.IdAcademicYear == param.IdAcademicYear
                                                         && e.IsAttendance || listIdLessonRealization.Contains(e.IdLesson))
                                                 .Select(e => e.IdLesson)
                                                 .ToListAsync(CancellationToken);

            var listSchedule = await _dbContext.Entity<MsSchedule>()
                          .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                          .Include(e => e.Sessions)
                          .Where(e => e.IdUser == param.IdUser
                                  && e.Lesson.IdAcademicYear == param.IdAcademicYear
                                  && listIdLessonByLessonTeacher.Contains(e.IdLesson) || listIdLessonRealization.Contains(e.IdLesson))
                          .Select(e => new
                          {
                              e.IdLesson,
                              idSubject = e.Lesson.Subject.Id,
                              subjectName = e.Lesson.Subject.Description,
                              e.Lesson.Subject.IdGrade,
                              e.Lesson.Semester,
                              e.Lesson.ClassIdGenerated,
                              e.IdDay,
                              e.Sessions.SessionID,
                              sessionIdDay = e.Sessions.IdDay,
                              StartTime = e.Sessions.StartTime,
                              EndTime = e.Sessions.EndTime,
                              e.IdUser,
                          })
                          .ToListAsync(CancellationToken);
            //listSchedule = listSchedule.Where(x => x.ClassIdGenerated == "6.12MAI.AG").ToList();
            var listIdLesson = listSchedule.Select(e => e.IdLesson).Distinct().ToList();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                         .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                         .ToListAsync(CancellationToken);

            var getStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                              .Include(e => e.Student)
                              .Where(e => e.IdAcademicYear == param.IdAcademicYear
                                      && e.ActiveStatus
                                      && e.StartDate.Date <= param.Date.Date
                                      && (e.EndDate == null || e.EndDate >= param.Date.Date)
                                      )
                              .Select(e => new
                              {
                                  e.IdStudent,
                                  e.StartDate,
                                  endDate = e.EndDate == null
                                             ? param.Date.Date
                                             : Convert.ToDateTime(e.EndDate),
                                  e.Student.IdBinusian
                              })
                              .ToListAsync(CancellationToken);

            var listIdStudent = getStudentStatus.Where(e => e.endDate >= param.Date).Select(e => e.IdStudent).ToList();

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Lessons)
                                .Where(e => (listIdLesson.Contains(e.IdLesson))
                                                && listIdStudent.Contains(e.HomeroomStudent.IdStudent) && e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear)
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
                                    IsDelete = false,
                                    IsFromMaster = true,
                                    IsShowHistory = false
                                })
                                .ToListAsync(CancellationToken);


            listHomeroomStudentEnrollment.ForEach(e =>
            {
                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Lessons)
                            .Include(e => e.LessonNew)
                            .Where(x => x.StartDate.Date <= param.Date.Date && x.LessonOld.IdAcademicYear == param.IdAcademicYear)
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
                                     Description = e.HomeroomStudent.Homeroom.Grade.Description
                                 },
                                 Semester = e.HomeroomStudent.Homeroom.Semester,
                                 IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                 EffectiveDate = e.StartDate,
                                 IsDelete = e.IsDelete,
                                 Datein = e.DateIn.Value,
                                 IsFromMaster = false,
                                 IsShowHistory= e.IsShowHistory,
                             })
                            .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                     .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                     .ToList();

            var listScheduleLesoonData = await _dbContext.Entity<MsScheduleLesson>()
                                .Include(e => e.Session)
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && e.ScheduleDate.Date == param.Date.Date
                                        && e.IdAcademicYear == param.IdAcademicYear
                                        && e.Lesson.Semester == param.Semester
                                        )
                                .Select(e => new
                                {
                                    e.Id,
                                    e.IdSession,
                                    sessionName = e.SessionID,
                                    e.ClassID,
                                    e.IdLesson,
                                    e.SessionID,
                                    sessionIdDay = e.Session.IdDay,
                                    StartTime = e.Session.StartTime,
                                    EndTime = e.Session.EndTime,
                                    e.IdDay,
                                    e.ScheduleDate
                                })
                                .ToListAsync(CancellationToken);

            var listScheduleLesoonDataB4FilterSchedule = listScheduleLesoonData;

            listScheduleLesoonData = listScheduleLesoonData.Where(x => listSchedule.Any(y => y.IdLesson == x.IdLesson && y.IdDay == x.IdDay && y.StartTime == x.StartTime && y.EndTime == x.EndTime && y.IdUser == param.IdUser)).ToList();

            var listScheduleLesoon = listScheduleLesoonData.Where(x => !scheduleRealization.Any(y => y.IdLesson == x.IdLesson && y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID)).ToList();

            foreach (var item in scheduleRealization)
            {
                if (listScheduleLesoonDataB4FilterSchedule.Any(x => x.IdLesson == item.IdLesson && x.SessionID == item.SessionID && x.ScheduleDate == item.ScheduleDate && !item.IsCancel))
                    listScheduleLesoon.Add(listScheduleLesoonDataB4FilterSchedule.Where(x => x.IdLesson == item.IdLesson && x.SessionID == item.SessionID && x.ScheduleDate == item.ScheduleDate && !item.IsCancel).FirstOrDefault());
            }
            listScheduleLesoon = listScheduleLesoon.Where(x => !scheduleRealizationChanged.Any(y => y.IdLesson == x.IdLesson && y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID)).ToList();

            var listIdScheduleLesson = listScheduleLesoon.Select(e => e.Id).ToList();

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                                .Where(e => listIdScheduleLesson.Contains(e.IdScheduleLesson) && (e.Status == AttendanceEntryStatus.Submitted || e.Status == AttendanceEntryStatus.Pending))
                                .Select(e => new
                                {
                                    e.IdScheduleLesson,
                                    e.DateIn,
                                    e.UserIn,
                                    e.UserUp,
                                    e.DateUp,
                                    e.IdHomeroomStudent,
                                    e.AttendanceMappingAttendance.Attendance.AbsenceCategory
                                })
                                .ToListAsync(CancellationToken);

            var listIdlevel = listHomeroomStudentEnrollment.Select(e => e.Level.Id).ToList();

            var listMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                .Where(e => listIdlevel.Contains(e.IdLevel) && e.AbsentTerms == AbsentTerm.Session)
                                .Select(e => new
                                {
                                    e.IdLevel,
                                    e.AbsentTerms
                                })
                                .ToListAsync(CancellationToken);

            List<SessionAttendanceV2Result> result = new List<SessionAttendanceV2Result>();
            for (var i = 0; i < listSchedule.Count(); i++)
            {
                var unsubmited = 0;
                var data = listSchedule[i];
                var semester = listPeriod
                               .Where(e => e.IdGrade == data.IdGrade && (e.StartDate.Date <= param.Date.Date && e.EndDate.Date >= param.Date.Date))
                               .Select(e => e.Semester).FirstOrDefault();
                if (data.Semester != semester)
                    continue;

                //moving student enrollment
                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, Convert.ToDateTime(param.Date), data.Semester.ToString(), data.IdLesson);

                var StudentEnrollmentMoving = listStudentEnrollmentMoving
                                                .Where(e => listIdStudent.Contains(e.IdStudent))
                                                .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate).ThenBy(e=> e.Datein)
                                                .ToList();

                var listStudent = StudentEnrollmentMoving.Select(e => e.IdHomeroomStudent).Distinct().ToList();
                var idLevel = StudentEnrollmentMoving.Select(e => e.Level.Id).FirstOrDefault();
                var idHomeroom = StudentEnrollmentMoving.Select(e => e.Homeroom.Id).FirstOrDefault();
                var term = listMappingAttendance.Where(e => e.IdLevel == idLevel).SingleOrDefault();
                if (term == null)
                    continue;//throw new BadRequestException($"Absent term with id level = {idLevel} is not found");

                var listIdScheduleLesoon = listScheduleLesoon
                                            .Where(e => e.IdLesson == data.IdLesson
                                                    && e.IdDay == data.IdDay
                                                    && e.sessionIdDay == data.sessionIdDay
                                                    && e.EndTime == data.EndTime
                                                    && e.StartTime == data.StartTime)
                                            .Select(e => e.Id).ToList();

                var session = listScheduleLesoon.Where(e => e.IdLesson == data.IdLesson && e.IdDay == data.IdDay && e.sessionIdDay == data.sessionIdDay && e.StartTime == data.StartTime && e.EndTime == data.EndTime)
                    .Select(e => new
                    {
                        e.IdSession,
                        e.sessionName,
                        e.ClassID,
                        e.Id,
                        e.SessionID
                    }).FirstOrDefault();

                if (session != null)
                {
                    var exsis = result.Where(e => e.ClassId == session.ClassID && e.Session.Id == session.IdSession).Any();

                    if (exsis)
                        continue;

                    var lastAt = listAttendanceEntry
                                .Where(e => listIdScheduleLesoon.Contains(e.IdScheduleLesson))
                                .OrderByDescending(e => e.DateIn)
                                .Select(e => e.DateIn)
                                .Max();

                    var lastByIdUserData = listAttendanceEntry
                                    .Where(e => listIdScheduleLesoon.Contains(e.IdScheduleLesson))
                                    .OrderByDescending(e => e.DateIn)
                                    .Select(e => new
                                    {
                                        idUser = e.UserUp ?? e.UserIn
                                    }).FirstOrDefault();

                    var lastByIdUser = string.Empty;

                    if (lastByIdUserData != null)
                        lastByIdUser = lastByIdUserData.idUser;

                    var lastBy = await _dbContext.Entity<MsUser>()
                                   .Where(e => e.Id == lastByIdUser)
                                   .Select(e => e.DisplayName)
                                   .FirstOrDefaultAsync(CancellationToken);

                    #region unsubmited
                    List<string> studentExcludeEnrollment = new List<string>();
                    var totalUnsubmitedStudentSession = 0;
                    for (var j = 0; j < listIdScheduleLesoon.Count(); j++)
                    {
                        var IdScheduleLesoon = listIdScheduleLesoon[j];
                        var ScheduleLesson = listScheduleLesoon.Where(e => e.Id == IdScheduleLesoon).FirstOrDefault();
                        var listAttendanceEntryExcludeStudent = listAttendanceEntry
                                                            .Where(e => e.IdScheduleLesson == session.Id)
                                                            .Select(e => e.IdHomeroomStudent)
                                                            .ToList();

                        studentExcludeEnrollment = listStudent
                                                        .Where(e => !listAttendanceEntryExcludeStudent
                                                        .Contains(e))
                                                        .ToList();

                        if (studentExcludeEnrollment.Any())
                        {
                            totalUnsubmitedStudentSession += studentExcludeEnrollment.Count();
                        }
                    }

                    unsubmited = studentExcludeEnrollment.Count();
                    #endregion

                    #region UnexcudesAbsen
                    var listAttendanceEntryUnexcudesAbsen = listAttendanceEntry
                                                           .Where(e =>
                                                               listIdScheduleLesoon.Contains(e.IdScheduleLesson) &&
                                                               e.AbsenceCategory == AbsenceCategory.Unexcused)
                                                           .Select(e => e.IdHomeroomStudent)
                                                           .Distinct()
                                                           .ToList();

                    var unexcusedAbsence = listAttendanceEntryUnexcudesAbsen.Count();
                    #endregion

                    result.Add(new SessionAttendanceV2Result
                    {
                        Subject = new ItemValueVm
                        {
                            Id = data.idSubject,
                            Description = data.subjectName
                        },
                        Session = new ItemValueVm
                        {
                            Id = session.IdSession,
                            Description = session.SessionID.ToString()
                        },
                        TotalStudent = listStudent.Count(),
                        Unsubmitted = unsubmited,
                        UnexcusedAbsence = unexcusedAbsence,
                        LastSavedBy = lastBy,
                        LastSavedAt = lastAt,
                        ClassId = session.ClassID,
                        IdScheduleLesson = session.Id,
                        IdHomeroom = idHomeroom
                    });
                }


            }

            result = result.OrderBy(e => e.Session.Description).ToList();
            return Request.CreateApiResult2(result as object);
        }


    }
}
