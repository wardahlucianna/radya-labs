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
    public class GetUnsubmittedAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetUnsubmittedAttendanceHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        private async Task<List<string>> GetStudentStatusByDate(string idStudent, DateTime date)
        {
            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus})
                .Where(x => x.IdStudent == idStudent && (x.StartDate == date.Date || x.EndDate == date.Date 
                    || (x.StartDate < date.Date
                        ? x.EndDate != null ? (x.EndDate > date.Date && x.EndDate < date.Date) || x.EndDate > date.Date : x.StartDate <= date.Date
                        : x.EndDate != null ? ((date.Date > x.StartDate && date.Date < x.EndDate) || date.Date > x.EndDate) : x.StartDate <= date.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

                List<string> studentId = new List<string>();

                foreach(var studentStatus in checkStudentStatus)
                {
                    if(studentStatus.EndDate != null)
                    {
                        if(date.Date <= studentStatus.EndDate)
                        {
                            studentId.Add(studentStatus.IdStudent);
                        }
                    }
                    else
                    {
                        studentId.Add(studentStatus.IdStudent);
                    }
                }

            if(studentId.Any())
                return studentId;
            
            return null;
        }
        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnresolvedAttendanceRequest>(nameof(GetUnresolvedAttendanceRequest.IdUser));
            var AcademicyearByDate = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
            .Where(x => _dateTime.ServerTime >= x.StartDate)
            .Where(x => _dateTime.ServerTime <= x.EndDate)
            .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
            .GroupBy(x => new
            {
                x.Grade.Level.IdAcademicYear
            })
            .Select(x => x.Key)
            .FirstOrDefaultAsync(CancellationToken);

            var allowIsAttendanceEntryByClassId = AcademicyearByDate != null ? await _dbContext.Entity<MsLessonTeacher>()
             .Include(x => x.Lesson)
             .Where(x => x.IdUser == param.IdUser)
             .Where(x => x.Lesson.IdAcademicYear == AcademicyearByDate.IdAcademicYear)
             .Where(x => x.IsAttendance)
             .Select(x => x.Lesson.Id)
             .ToListAsync(CancellationToken) : new List<string>();

            var currentAY = await _dbContext.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
               .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
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

                var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => true);

                if(currentAY == null)
                {
                    predicate.And(x => x.Homeroom.IdAcademicYear == lastAY.Id);
                }
                else
                {
                    predicate.And(x => x.Homeroom.IdAcademicYear == currentAY.Id);
                }

                var attendances = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                              .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                                              .Include(x => x.GeneratedScheduleStudent)
                                              .Include(x => x.AttendanceEntries)
                                              .Where(x => x.IsGenerated
                                                          && x.ScheduleDate <= _dateTime.ServerTime
                                                          && (x.IdUser == param.IdUser || allowIsAttendanceEntryByClassId.Contains(x.IdLesson))
                                                          && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session)
                                                          )
                                              .Where(predicate)
                                              .Where(x => !x.AttendanceEntries.Any())
                                              .OrderByDescending(x => x.ScheduleDate)
                                              .Select(x => new UnresolvedAttendanceResult
                                              {
                                                  Date = x.ScheduleDate,
                                                  ClassID = x.ClassID,
                                                  Teacher = new ItemValueVm
                                                  {
                                                      Id = x.IdUser,
                                                      Description = x.TeacherName
                                                  },
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
                                                  },
                                                  IdStudent = x.GeneratedScheduleStudent.IdStudent
                                              }).ToListAsync();

            var attendancesExist = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                              .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                              .Include(x => x.GeneratedScheduleStudent)
                              .Include(x => x.AttendanceEntries)
                              .Where(x => x.IsGenerated
                                          && x.ScheduleDate <= _dateTime.ServerTime
                                          && (x.IdUser == param.IdUser || allowIsAttendanceEntryByClassId.Contains(x.IdLesson))
                                          && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session)
                                          )
                              .Where(predicate)
                              .Where(x => x.AttendanceEntries.Any())
                              .OrderByDescending(x => x.ScheduleDate)
                              .Select(x => new UnresolvedAttendanceResult
                              {
                                  Date = x.ScheduleDate,
                                  ClassID = x.ClassID,
                                  Teacher = new ItemValueVm
                                  {
                                      Id = x.IdUser,
                                      Description = x.TeacherName
                                  },
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
                                  },
                                  IdStudent = x.GeneratedScheduleStudent.IdStudent
                              }).ToListAsync();

            if (attendances.Count != 0)
            {
                attendances = attendances.Where(x => x.Teacher.Id == param.IdUser).ToList();
            }

            if (attendancesExist.Count != 0)
            {
                var attendanceCompareExist = new List<UnresolvedAttendanceResult>();

                foreach(var item in attendances)
                {
                    if (!attendancesExist.Any(x => x.IdStudent == item.IdStudent && x.Session.Id == item.Session.Id && x.Date == item.Date))
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
            //     .Where(x => x.IdAcademicYear == AcademicyearByDate.IdAcademicYear && (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date 
            //         || (x.StartDate < _dateTime.ServerTime.Date
            //             ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
            //             : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
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

            if (param.IdSchool != "1")
            {
                return Request.CreateApiResult2(new GetUnresolvedAttendanceResult
                {
                    IsShowingPopup = Convert.ToBoolean(groupShowingPopup),
                    Attendances = attendances.GroupBy(x => new { x.Date, x.ClassID, TeacherId = x.Teacher.Id, SubjectId = x.Subject.Id, HomeroomId = x.Homeroom.Id, SessionId = x.Session.Id })
                                         .Select(x => new UnresolvedAttendanceGroupResult
                                         {
                                             Date = x.Key.Date,
                                             ClassID = x.Key.ClassID,
                                             Teacher = new ItemValueVm
                                             {
                                                 Id = x.Key.TeacherId,
                                                 Description = x.FirstOrDefault().Teacher.Description
                                             },
                                             Subject = new ItemValueVm
                                             {
                                                 Id = x.Key.SubjectId,
                                                 Description = x.FirstOrDefault().Subject.Description
                                             },
                                             Homeroom = new ItemValueVm
                                             {
                                                 Id = x.Key.HomeroomId,
                                                 Description = x.FirstOrDefault().Homeroom.Description
                                             },
                                             Session = new ItemValueVm
                                             {
                                                 Id = x.Key.SessionId,
                                                 Description = x.FirstOrDefault().Session.Description
                                             },
                                             Level = x.FirstOrDefault().Level,
                                             TotalStudent = x.Count()
                                         }).OrderBy(x => x.Date)
                                                    .ThenBy(x => int.TryParse(x.Session.Description, out var sessionNumber) ? sessionNumber : 0)
                                         .ToList(),
                    // EventAttendance = eventAttendances
                } as object);
            }
            else
            {
                return Request.CreateApiResult2(new GetUnresolvedAttendanceResult
                {
                    IsShowingPopup = Convert.ToBoolean(groupShowingPopup),
                    Attendances = attendances.GroupBy(x => new { x.Date, x.ClassID, TeacherId = x.Teacher.Id, SubjectId = x.Subject.Id,  SessionId = x.Session.Id })
                                        .Select(x => new UnresolvedAttendanceGroupResult
                                        {
                                            Date = x.Key.Date,
                                            ClassID = x.Key.ClassID,
                                            Teacher = new ItemValueVm
                                            {
                                                Id = x.Key.TeacherId,
                                                Description = x.FirstOrDefault().Teacher.Description
                                            },
                                            Subject = new ItemValueVm
                                            {
                                                Id = x.Key.SubjectId,
                                                Description = x.FirstOrDefault().Subject.Description
                                            },
                                            Homeroom = new ItemValueVm
                                            {
                                                Id = "",
                                                Description = ""
                                            },
                                            Session = new ItemValueVm
                                            {
                                                Id = x.Key.SessionId,
                                                Description = x.FirstOrDefault().Session.Description
                                            },
                                            Level = x.FirstOrDefault().Level,
                                            TotalStudent = x.Count()
                                        }).OrderBy(x => x.Date)
                                                   .ThenBy(x => int.TryParse(x.Session.Description, out var sessionNumber) ? sessionNumber : 0)
                                        .ToList(),
                    // EventAttendance = eventAttendances
                } as object);
            }            
        }
    }
}
