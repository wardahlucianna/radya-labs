using System;
using System.Linq;
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
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetPendingAttendanceTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetPendingAttendanceTermDayHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnresolvedAttendanceRequest>(nameof(GetUnresolvedAttendanceRequest.IdUser));
            
            // var currentAY = await _dbContext.Entity<MsPeriod>()
            //    .Include(x => x.Grade)
            //        .ThenInclude(x => x.Level)
            //            .ThenInclude(x => x.AcademicYear)
            //    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
            //    .Where(x => _dateTime.ServerTime >= x.StartDate)
            //    .Where(x => _dateTime.ServerTime <= x.EndDate)
            //    .Select(x => new
            //    {
            //        Id = x.Grade.Level.AcademicYear.Id
            //    }).FirstOrDefaultAsync();

            var attendances = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                              .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                                              .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                                              .Where(x => x.IsGenerated
                                                          && x.Homeroom.HomeroomTeachers.Any(y => y.IdBinusian == param.IdUser && y.IsAttendance)
                                                          && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day))
                                              .Include(x => x.AttendanceEntries)
                                              .Where(x => x.AttendanceEntries.Any(x => x.Status == AttendanceEntryStatus.Pending))
                                              .OrderByDescending(x => x.ScheduleDate)
                                              .Select(x => new UnresolvedAttendanceResult
                                              {
                                                  Date = x.ScheduleDate,
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
                                             TotalStudent = x.Count()
                                         }).OrderBy(x => x.Date)
                                                    .ThenBy(x => int.TryParse(x.Session.Description, out var sessionNumber) ? sessionNumber : 0)
                                         .ToList(),
                // EventAttendance = eventAttendances
            } as object);
        }
    }
}
