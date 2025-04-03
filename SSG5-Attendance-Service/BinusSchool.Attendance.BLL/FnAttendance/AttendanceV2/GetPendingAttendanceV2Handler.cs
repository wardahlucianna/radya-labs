using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetPendingAttendanceV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetimeNow;
        public GetPendingAttendanceV2Handler(IAttendanceDbContext dbContext, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnresolvedAttendanceV2Request>();

            var listHomeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(e => e.IdBinusian == param.IdUser
                                        && e.Homeroom.IdAcademicYear == param.IdAcademicYear
                                        )
                                .Select(e => new
                                {
                                    e.IdHomeroom,
                                    homeroom = $"{e.Homeroom.Grade.Code}{e.Homeroom.GradePathwayClassroom.Classroom.Code}",
                                    e.Homeroom.Grade.IdLevel,
                                    level = e.Homeroom.Grade.Level.Description,
                                    levelCode = e.Homeroom.Grade.Level.Code,
                                    e.Homeroom.IdGrade
                                })
                                .ToListAsync(CancellationToken);

            var listIdHomeroom = listHomeroom.Select(e => e.IdHomeroom).Distinct().ToList();
            var listIdlevel = listHomeroom.Select(e => e.IdLevel).Distinct().ToList();
            var listIdGrade = listHomeroom.Select(e => e.IdGrade).Distinct().ToList();

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                               .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.ActiveStatus)
                               .Select(e => new
                               {
                                   e.IdStudent,
                                   e.StartDate,
                                   endDate = e.EndDate == null
                                           ? _datetimeNow.ServerTime.Date
                                           : Convert.ToDateTime(e.EndDate),
                                   e.Student.IdBinusian
                               })
                               .ToListAsync(CancellationToken);

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                      .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                      .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                .Where(e => listIdHomeroom.Contains(e.HomeroomStudent.IdHomeroom) && e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear)
                                .GroupBy(e => new
                                {
                                    e.HomeroomStudent.IdStudent,
                                    e.IdLesson,
                                    e.IdHomeroomStudent,
                                    e.HomeroomStudent.IdHomeroom,
                                    IdHomeroomStudentEnrollment = e.Id,
                                    e.HomeroomStudent.Homeroom.IdGrade,
                                    e.HomeroomStudent.Homeroom.Semester
                                })
                                .Select(e => new GetHomeroom
                                {
                                    IdLesson = e.Key.IdLesson,
                                    IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                    Homeroom = new ItemValueVm
                                    {
                                        Id = e.Key.IdHomeroom,
                                    },
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = e.Key.IdGrade
                                    },
                                    IdStudent = e.Key.IdStudent,
                                    IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                    IsFromMaster = true,
                                    IsDelete = false,
                                    Semester = e.Key.Semester,
                                    IsShowHistory = false
                                })
                                .ToListAsync(CancellationToken);

            listHomeroomStudentEnrollment.ForEach(e =>
            {
                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                           .Include(e => e.LessonNew)
                           .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                           .Where(x => x.StartDate.Date <= _datetimeNow.ServerTime.Date && x.LessonOld.IdAcademicYear == param.IdAcademicYear)
                           .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                              .Select(e => new GetHomeroom
                              {
                                  IdLesson = e.IdLessonNew,
                                  IdHomeroomStudent = e.IdHomeroomStudent,
                                  Homeroom = new ItemValueVm
                                  {
                                      Id = e.HomeroomStudent.IdHomeroom,
                                  },
                                  IdStudent = e.HomeroomStudent.IdStudent,
                                  IsFromMaster = false,
                                  EffectiveDate = e.StartDate,
                                  IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                  IsDelete = e.IsDelete,
                                  Semester = e.HomeroomStudent.Homeroom.Semester,
                                  Datein = e.DateIn.Value,
                                  IsShowHistory = e.IsShowHistory,
                              })
                           .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                   .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                   .ToList();

            var getIdHomeroomStudentEnrollment = getTrHomeroomStudentEnrollment
                                                    .Select(e => e.IdHomeroomStudentEnrollment)
                                                    .Distinct().ToList();


            var listIdLesson = listStudentEnrollmentUnion.Select(f => f.IdLesson).Distinct().ToList();

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                               .Include(e => e.ScheduleLesson)
                               .Include(e => e.HomeroomStudent)
                               .Where(e => e.Status == AttendanceEntryStatus.Pending && e.ScheduleLesson.IdAcademicYear == param.IdAcademicYear && listIdLesson.Contains(e.ScheduleLesson.IdLesson))
                               .Select(e => new
                               {
                                   e.IdScheduleLesson,
                                   e.IdHomeroomStudent,
                                   e.HomeroomStudent.IdHomeroom
                               })
                               .ToListAsync(CancellationToken);

            var listIdScheduleLesson = listAttendanceEntry.Select(e => e.IdScheduleLesson).ToList();

            var listScheduleLesoon = await _dbContext.Entity<MsScheduleLesson>()
                                .Include(e => e.Subject)
                                .Include(e => e.Session)
                                .Include(e => e.Lesson)
                                .Where(e => listIdScheduleLesson.Contains(e.Id)
                                        && e.IdAcademicYear == param.IdAcademicYear)
                                .Select(e => new
                                {
                                    e.Id,
                                    e.ScheduleDate,
                                    e.IdLesson,
                                    e.ClassID,
                                    idSession = e.Session.Id,
                                    sessionName = e.Session.Name,
                                    e.IdGrade,
                                    e.IdWeek,
                                    e.IdDay,
                                    e.Lesson.Semester
                                })
                                .ToListAsync(CancellationToken);

            var listSchedule = await _dbContext.Entity<MsSchedule>()
                               .Include(e => e.Lesson)
                               .Include(e => e.User)
                               .Where(e => listIdLesson.Contains(e.IdLesson)
                                       && e.Lesson.IdAcademicYear == param.IdAcademicYear)
                               .Select(e => new
                               {
                                   e.Id,
                                   e.IdLesson,
                                   e.Lesson.IdSubject,
                                   e.IdWeek,
                                   e.IdDay,
                                   e.IdUser,
                               })
                               .ToListAsync(CancellationToken);

            var listMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                .Where(e => listIdlevel.Contains(e.IdLevel))
                                .Select(e => new
                                {
                                    e.IdLevel,
                                    e.AbsentTerms
                                })
                                .ToListAsync(CancellationToken);

            var countStudent = listStudentEnrollmentUnion.Select(e => e.IdHomeroomStudent).Distinct().Count();
            List<UnresolvedAttendanceGroupV2Result> attendance = new List<UnresolvedAttendanceGroupV2Result>();
            for (var i = 0; i < listScheduleLesoon.Count(); i++)
            {
                var data = listScheduleLesoon[i];
                var semester = listPeriod
                                .Where(e => e.IdGrade == data.IdGrade && (e.StartDate.Date >= data.ScheduleDate.Date && e.EndDate.Date <= data.ScheduleDate.Date))
                                .Select(e => e.Semester).ToList();

                var listStatusStudentByDate = listStudentStatus
                                            .Where(e => e.StartDate.Date <= data.ScheduleDate.Date
                                                        && e.endDate.Date >= data.ScheduleDate.Date)
                                            .Select(e => e.IdStudent).ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, data.ScheduleDate, data.Semester.ToString(), data.IdLesson);



                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                               .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                               .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                               .ToList();

                var listIdStudentMoving = studentEnrollmentMoving.Select(e => e.IdHomeroomStudent).ToList();
                var listAttendanceEntryBySchedule = listAttendanceEntry.Where(e => e.IdScheduleLesson == data.Id && listIdStudentMoving.Contains(e.IdHomeroomStudent)).ToList();
                var HomeroomBySchedule = listStudentEnrollmentUnion.Where(e => e.IdLesson == data.IdLesson).Select(e => e.Homeroom.Id).FirstOrDefault();
                var homeroom = listHomeroom.Where(e => e.IdHomeroom == HomeroomBySchedule).FirstOrDefault();

                if (homeroom == null)
                    continue;

                var queryAttendanceEntryByScheduleByHomeroom = listAttendanceEntryBySchedule.Distinct();
                if (param.CurrentPosition == "CA")
                    queryAttendanceEntryByScheduleByHomeroom = queryAttendanceEntryByScheduleByHomeroom.Where(e => e.IdHomeroom == HomeroomBySchedule);

                var listAttendanceEntryByScheduleByHomeroom = queryAttendanceEntryByScheduleByHomeroom.ToList();

                if (listAttendanceEntryByScheduleByHomeroom.Any())
                {
                    attendance.Add(new UnresolvedAttendanceGroupV2Result
                    {
                        Date = data.ScheduleDate.Date,
                        ClassID = data.ClassID,
                        Homeroom = new ItemValueVm
                        {
                            Id = homeroom.IdHomeroom,
                            Description = homeroom.homeroom
                        },
                        Session = new ItemValueVm
                        {
                            Id = data.idSession,
                            Description = data.sessionName,
                        },
                        TotalStudent = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any()
                                                 ? listAttendanceEntryBySchedule.Select(e=>e.IdHomeroomStudent).Distinct().Count()
                                                 : countStudent,
                        IdLesson = data.IdLesson
                    });
                }
            }

            var result = new GetUnresolvedAttendanceV2Result();
            if (param.CurrentPosition == PositionConstant.ClassAdvisor)
            {
                result.IsShowingPopup = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any();
                result.Attendances = result.IsShowingPopup
                                        ? attendance.OrderBy(e => e.Date).ToList()
                                        : null;
                result.TotalUnsubmitted = result.Attendances == null ? 0: result.Attendances.Select(e => e.TotalStudent).Sum();
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
