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
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetPendingAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetPendingAttendanceHandler(
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
                                              .Include(x => x.GeneratedScheduleStudent)
                                              .Include(x => x.AttendanceEntries)
                                              .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers)
                                              .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                                              .Where(x => x.IsGenerated
                                                          && x.Homeroom.HomeroomTeachers.Any(y => y.IdBinusian == param.IdUser && y.IsAttendance)
                                                          && x.AttendanceEntries.Any(x => x.Status == AttendanceEntryStatus.Pending)
                                                          && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
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
                                                      Description = x.Homeroom.Grade.Level.Description,
                                                  },
                                                  IdStudent = x.GeneratedScheduleStudent.IdStudent
                                              }).ToListAsync();

            if(attendances.Count > 0)
            {
                var paramDate = attendances.OrderBy(x => x.Date).Select(x => x.Date).First();

                var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear })
                .Where(x => x.ActiveStatus == false && x.CurrentStatus == "A" && (x.StartDate == paramDate.Date || x.EndDate == paramDate.Date
                    || (x.StartDate < paramDate.Date
                        ? x.EndDate != null ? (x.EndDate > paramDate.Date && x.EndDate < paramDate.Date) || x.EndDate > paramDate.Date : x.StartDate <= paramDate.Date
                        : x.EndDate != null ? ((paramDate.Date > x.StartDate && paramDate.Date < x.EndDate) || paramDate.Date > x.EndDate) : x.StartDate <= paramDate)))
                .ToListAsync();

                if (checkStudentStatus != null)
                {
                    attendances = attendances.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();
                }
            }

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
                EventAttendance = null
            } as object);
        }
    }
}
