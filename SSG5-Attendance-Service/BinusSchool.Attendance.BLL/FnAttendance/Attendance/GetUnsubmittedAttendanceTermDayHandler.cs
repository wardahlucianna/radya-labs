using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetUnsubmittedAttendanceTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetUnsubmittedAttendanceTermDayHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnresolvedAttendanceRequest>(nameof(GetUnresolvedAttendanceRequest.IdUser));

            var currentAY = await _dbContext.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
               .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
               .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
               .Select(x => new
               {
                   Id = x.Grade.Level.AcademicYear.Id
               }).FirstOrDefaultAsync();

            var lastAY = await _dbContext.Entity<MsAcademicYear>()
               .Select(x => new
               {
                   Id = x.Id,
                   Code = x.Code
               })
               .OrderByDescending(x => x.Code)
               .FirstOrDefaultAsync();

            var predicateSearch = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);

            if (param.IdSchool == "2")
            {
                predicateSearch = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.IdUser == param.IdUser);
            }

            var attendances = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                              .Include(x => x.GeneratedScheduleStudent)
                                              .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                                              .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                                              .Where(x => x.IsGenerated
                                                          && x.ScheduleDate <= _dateTime.ServerTime.Date
                                                          && x.Homeroom.HomeroomTeachers.Any(y => y.IdBinusian == param.IdUser && y.IsAttendance)
                                                          //&& x.IdUser == param.IdUser
                                                          && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day))
                                              .Where(predicateSearch)
                                              .Include(x => x.AttendanceEntries)
                                              .Where(x => !x.AttendanceEntries.Any())
                                              .OrderByDescending(x => x.ScheduleDate)
                                              .Select(x => new UnresolvedAttendanceResult
                                              {
                                                  Date = x.ScheduleDate,
                                                  IdStudent = x.GeneratedScheduleStudent.IdStudent,
                                                  ClassID = x.ClassID,
                                                  Teacher = x.Homeroom.HomeroomTeachers.Where(y => y.IdBinusian == param.IdUser && y.IsAttendance).Select(y => new ItemValueVm
                                                  {
                                                      Id = y.IdBinusian,
                                                      Description = $"{y.Staff.FirstName} {y.Staff.LastName}"
                                                  }).First(),
                                                  Subject = new ItemValueVm
                                                  {
                                                      Id = x.IdSubject,
                                                      Description = x.SubjectName
                                                  },
                                                  Homeroom = new ItemValueVm
                                                  {
                                                      Id = x.IdHomeroom,
                                                      Description = x.HomeroomName
                                                  },
                                                  Session = new ItemValueVm
                                                  {
                                                      Id = x.IdSession,
                                                      Description = x.SessionID
                                                  },
                                                  Level = new CodeWithIdVm
                                                  {
                                                      Id = x.Homeroom.Grade.Level.Id,
                                                      Code = x.Homeroom.Grade.Level.Code,
                                                      Description = x.Homeroom.Grade.Level.Description
                                                  }
                                              }).ToListAsync();

            var attendancesExist = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                  .Include(x => x.GeneratedScheduleStudent)
                                  .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                                  .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                                  .Where(x => x.IsGenerated
                                              && x.ScheduleDate <= _dateTime.ServerTime.Date
                                              && x.Homeroom.HomeroomTeachers.Any(y => y.IdBinusian == param.IdUser && y.IsAttendance)
                                              //&& x.IdUser == param.IdUser
                                              && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day))
                                  .Where(predicateSearch)
                                  .Include(x => x.AttendanceEntries)
                                  .Where(x => x.AttendanceEntries.Any())
                                  .OrderByDescending(x => x.ScheduleDate)
                                  .Select(x => new UnresolvedAttendanceResult
                                  {
                                      Date = x.ScheduleDate,
                                      IdStudent = x.GeneratedScheduleStudent.IdStudent,
                                      ClassID = x.ClassID,
                                      Teacher = x.Homeroom.HomeroomTeachers.Where(y => y.IdBinusian == param.IdUser && y.IsAttendance).Select(y => new ItemValueVm
                                      {
                                          Id = y.IdBinusian,
                                          Description = $"{y.Staff.FirstName} {y.Staff.LastName}"
                                      }).First(),
                                      Subject = new ItemValueVm
                                      {
                                          Id = x.IdSubject,
                                          Description = x.SubjectName
                                      },
                                      Homeroom = new ItemValueVm
                                      {
                                          Id = x.IdHomeroom,
                                          Description = x.HomeroomName
                                      },
                                      Session = new ItemValueVm
                                      {
                                          Id = x.IdSession,
                                          Description = x.SessionID
                                      },
                                      Level = new CodeWithIdVm
                                      {
                                          Id = x.Homeroom.Grade.Level.Id,
                                          Code = x.Homeroom.Grade.Level.Code,
                                          Description = x.Homeroom.Grade.Level.Description
                                      }
                                  }).ToListAsync();

            if (attendances.Count != 0)
            {
                var selectTeacher = param.IdUser;

                attendances = attendances.Where(x => x.Teacher.Id == selectTeacher).ToList();
                attendancesExist = attendancesExist.Where(x => x.Teacher.Id == selectTeacher).ToList();
            }

            if (attendancesExist.Count != 0)
            {
                var attendanceCompareExist = new List<UnresolvedAttendanceResult>();

                foreach (var item in attendances)
                {
                    if (!attendancesExist.Any(x => x.IdStudent == item.IdStudent && x.Date == item.Date))
                        attendanceCompareExist.Add(item);
                }

                attendances = attendanceCompareExist;
            }

            var Attendance = new List<UnresolvedAttendanceResult>();

            var AcademicYear = currentAY.Id != null ? currentAY.Id : lastAY.Id;

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear })
            .Where(x => x.IdAcademicYear == AcademicYear)
            .ToListAsync();

            foreach (var item in attendances)
            {
                var checkStudentStatusStudent = checkStudentStatus
                .Where(x => x.IdStudent == item.IdStudent && (x.StartDate == item.Date || x.EndDate == item.Date
                    || (x.StartDate < item.Date
                        ? x.EndDate != null ? (x.EndDate > item.Date && x.EndDate < item.Date) || x.EndDate > item.Date : x.StartDate <= item.Date
                        : x.EndDate != null ? ((item.Date > x.StartDate && item.Date < x.EndDate) || item.Date > x.EndDate) : x.StartDate <= item.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToList();

                if (checkStudentStatusStudent.Count == 0)
                    Attendance.Add(item);
            }

            attendances = Attendance;

            // var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear})
            //     .Where(x => x.IdAcademicYear == currentAY.Id && (x.StartDate ==_dateTime.ServerTime.Date || x.EndDate ==_dateTime.ServerTime.Date 
            //         || (x.StartDate <_dateTime.ServerTime.Date
            //             ? x.EndDate != null ? (x.EndDate >_dateTime.ServerTime.Date && x.EndDate <_dateTime.ServerTime.Date) || x.EndDate >_dateTime.ServerTime.Date : x.StartDate <=_dateTime.ServerTime.Date
            //             : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate &&_dateTime.ServerTime.Date < x.EndDate) ||_dateTime.ServerTime.Date > x.EndDate) : x.StartDate <=_dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
            //     .ToListAsync();

            // List<string> studentId = new List<string>();

            // foreach(var studentStatus in checkStudentStatus)
            // {
            //     if(studentStatus.EndDate != null)
            //     {
            //         if(_dateTime.ServerTime.Date <= studentStatus.EndDate)
            //         {
            //             studentId.Add(studentStatus.IdStudent);
            //         }
            //     }
            //     else
            //     {
            //         studentId.Add(studentStatus.IdStudent);
            //     }
            // }

            // if(checkStudentStatus != null)
            // {
            //     if(studentId.Any())
            //         attendances = attendances.Where(x => !studentId.ToList().Contains(x.IdStudent)).ToList();
            // }

            // var eventAttendances = await _dbContext.Entity<MsEvent>()
            //                                        .Include(x => x.EventDetails)
            //                                        .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGradeStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers)
            //                                        .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForSubjectStudents).ThenInclude(x => x.Lesson).ThenInclude(x => x.LessonTeachers)
            //                                        .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents)
            //                                        .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents).ThenInclude(x => x.UserEventAttendances)
            //                                        .Where(x => x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.IsSetAttendance)
            //                                                    && x.EventDetails.Any(y => y.StartDate.Date <= _dateTime.ServerTime.Date)
            //                                                    && x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAtdCheckStudents.Any(z => !z.UserEventAttendances.Any()))
            //                                                    && x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAtdPICStudents.Any(z => z.IdUser == param.IdUser)))
            //                                        //|| (x.EventIntendedFor.EventIntendedForGradeStudents.Any(y => y.Homeroom.HomeroomTeachers.Any(z => z.IdUser == param.IdUser && z.IsAttendance)) && x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAttendancePICStudents.Any(z => z.Type == EventIntendedForAttendancePICStudent.Homeroom)))
            //                                        //|| (x.EventIntendedFor.EventIntendedForSubjectStudents.Any(y => y.Lesson.LessonTeachers.Any(z => z.IdUser == param.IdUser && z.IsAttendance)) && x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAttendancePICStudents.Any(z => z.Type == EventIntendedForAttendancePICStudent.Subject)))
            //                                        //|| (param.IsStaff && x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAttendancePICStudents.Any(z => z.Type == EventIntendedForAttendancePICStudent.Staff)))))
            //                                        .Select(x => new UnresolvedEventAttendanceResult
            //                                        {
            //                                            Id = x.Id,
            //                                            Description = x.Name,
            //                                            Dates = x.EventDetails.Select(y => new EventDate
            //                                            {
            //                                                StartDate = y.StartDate,
            //                                                StartTime = y.StartDate.TimeOfDay,
            //                                                EndDate = y.EndDate,
            //                                                EndTime = y.EndDate.TimeOfDay
            //                                            }).ToList(),
            //                                            EventCheck = x.EventIntendedFor.EventIntendedForAttendanceStudents
            //                                                          .SelectMany(y => y.EventIntendedForAtdCheckStudents.Select(z => new EventCheck
            //                                                          {
            //                                                              Id = z.Id,
            //                                                              Description = z.CheckName,
            //                                                              Date = z.StartDate,
            //                                                              AttendanceTime = z.Time
            //                                                          }))
            //                                                          .ToList()
            //                                        })
            //                                        .ToListAsync();
            var idLevels = attendances.Select(x => x.Level.Id).Distinct().ToList();
            var showingPopupByLevel = await _dbContext.Entity<MsMappingAttendance>()

                .Where(x => idLevels.Contains(x.IdLevel))
                .GroupBy(x => x.ShowingModalReminderAttendanceEntry)
                .Select(x => x.Key)
                .ToListAsync(CancellationToken);

            var groupShowingPopup = showingPopupByLevel.GroupBy(x => x).Select(x => x.Max(y => Convert.ToInt32(y))).FirstOrDefault();

            return Request.CreateApiResult2(new GetUnresolvedAttendanceResult
            {
                IsShowingPopup = Convert.ToBoolean(groupShowingPopup),
                Attendances = attendances.GroupBy(x => new { x.Date, TeacherId = x.Teacher.Id, HomeroomId = x.Homeroom.Id })
                                         .Select(x => new UnresolvedAttendanceGroupResult
                                         {
                                             Date = x.Key.Date,
                                             Teacher = new ItemValueVm
                                             {
                                                 Id = x.Key.TeacherId,
                                                 Description = x.FirstOrDefault().Teacher.Description
                                             },
                                             Homeroom = new ItemValueVm
                                             {
                                                 Id = x.Key.HomeroomId,
                                                 Description = x.FirstOrDefault().Homeroom.Description
                                             },
                                             TotalStudent = x.GroupBy(y => y.IdStudent).Count()
                                         }).OrderBy(x => x.Date)
                                         .ToList(),
                // EventAttendance = eventAttendances
            } as object);
        }
    }
}
