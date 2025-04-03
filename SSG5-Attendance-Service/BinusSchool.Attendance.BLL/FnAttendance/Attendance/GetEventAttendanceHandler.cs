using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
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
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetEventAttendanceHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetEventAttendanceRequest.Date),
            nameof(GetEventAttendanceRequest.IdUser)
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetEventAttendanceHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventAttendanceRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                 //var period ==> semester where dari date
                var predicateStudent = PredicateBuilder.Create<TrEventIntendedForAttendanceStudent>(x
                    => (x.IsSetAttendance && x.EventIntendedForAtdPICStudents.Any(y => y.IdUser == param.IdUser))
                    || x.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == param.IdUser)));
                predicateStudent = predicateStudent.And(x => x.EventIntendedFor.Event.EventDetails.Any(y
                    => EF.Functions.DateDiffDay(y.StartDate, param.Date) >= 0
                    && EF.Functions.DateDiffDay(y.EndDate, param.Date) <= 0));
                predicateStudent = predicateStudent.And(x => x.EventIntendedFor.IntendedFor == RoleConstant.Student);

                predicateStudent = predicateStudent.And(x => x.EventIntendedFor.Event.StatusEvent == ApprovalStatus.Approved.ToString());
                predicateStudent = predicateStudent.And(x => x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(f=>f.IsSetAttendance));

                DateTime StartDate = default;
                DateTime EndDate = default;
                
                if (!string.IsNullOrEmpty(param.IdHomeroom))
                {
                    var GetSemester = await _dbContext.Entity<MsHomeroom>()
                        .Where(x => x.Id == param.IdHomeroom)
                        .FirstOrDefaultAsync(CancellationToken);

                    var GetPeriod = await _dbContext.Entity<MsPeriod>()
                            .Where(x => x.IdGrade == GetSemester.IdGrade && x.Semester == GetSemester.Semester)
                            .ToListAsync(CancellationToken);

                    StartDate = GetPeriod.Select(e => e.StartDate).Min();
                    EndDate = GetPeriod.Select(e => e.EndDate).Max();

                    var idStudents = await _dbContext.Entity<MsHomeroomStudent>()
                        .Where(x => x.IdHomeroom == param.IdHomeroom)
                        .Select(x => x.IdStudent)
                        .ToListAsync(CancellationToken);

                    if (idStudents.Count != 0)
                        predicateStudent = predicateStudent.And(x => x.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => idStudents.Contains(z.IdUser))));
                }

                var attendanceStudents = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails)
                    .Include(x => x.EventIntendedForAtdPICStudents)
                    .Include(x => x.EventIntendedForAtdCheckStudents).ThenInclude(x => x.UserEventAttendance2s)
                    .Where(predicateStudent)
                    .ToListAsync(CancellationToken);

                var QuesryResults2 = attendanceStudents
                    .Select(x => new GetEventAttendanceResult
                    {
                        Id = x.EventIntendedFor.IdEvent,
                        Name = x.EventIntendedFor.Event.Name,
                        Date = new DateTimeRange
                        {
                            Start = x.EventIntendedFor.Event.EventDetails.Min(y => y.StartDate),
                            End = x.EventIntendedFor.Event.EventDetails.Max(y => y.EndDate)
                        },
                        UnsaveAbsence = 0,
                        UnexcuseAbsence = 0,
                        Audit = new AuditableResult
                        {
                            UserIn = x.EventIntendedForAtdCheckStudents.Any(y => y.UserEventAttendance2s.Count != 0)
                                ? new AuditableUser(x.EventIntendedForAtdCheckStudents.Min(y => y.UserEventAttendance2s.FirstOrDefault()?.UserIn))
                                : null,
                            DateIn = x.EventIntendedForAtdCheckStudents.Any(y => y.UserEventAttendance2s.Count != 0)
                                ? x.EventIntendedForAtdCheckStudents.Min(y => y.UserEventAttendance2s.FirstOrDefault()?.DateIn)
                                : null,
                            UserUp = x.EventIntendedForAtdCheckStudents.Any(y => y.UserEventAttendance2s.Any(z => z.UserUp != null))
                                ? new AuditableUser(x.EventIntendedForAtdCheckStudents.Max(y => y.UserEventAttendance2s.FirstOrDefault()?.UserIn))
                                : null,
                            DateUp = x.EventIntendedForAtdCheckStudents.Any(y => y.UserEventAttendance2s.Any(z => z.UserUp != null))
                                ? x.EventIntendedForAtdCheckStudents.Max(y => y.UserEventAttendance2s.FirstOrDefault()?.DateIn)
                                : null
                        }
                    });

                if (!string.IsNullOrEmpty(param.IdHomeroom))
                {
                    QuesryResults2 = QuesryResults2.Where(e => (e.Date.Start.Date >= StartDate.Date && e.Date.Start.Date <= EndDate.Date) && (e.Date.End.Date >= StartDate.Date && e.Date.End.Date <= EndDate.Date));
                }

                var results2 = QuesryResults2.ToList();

                if (results2.Count != 0)
                {
                    var idEvents = results2.Select(x => x.Id);
                    var attendances = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                        .Where(x
                            => idEvents.Contains(x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id)
                            && EF.Functions.DateDiffDay(x.StartDate, param.Date) == 0
                            && x.IsPrimary)
                        .ToListAsync(CancellationToken);

                    foreach (var attendance in attendances)
                    {
                        attendance.EventIntendedForAttendanceStudent = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                                                                        .Include(x => x.EventIntendedFor)
                                                                        .ThenInclude(x => x.Event)
                                                                        .ThenInclude(x => x.EventDetails)
                                                                        .ThenInclude(x => x.UserEvents)
                                                                        .Where(x => x.Id == attendance.IdEventIntendedForAttendanceStudent)
                                                                        .FirstOrDefaultAsync(CancellationToken);

                        attendance.UserEventAttendance2s = await _dbContext.Entity<TrUserEventAttendance2>()
                                                            .Include(x => x.UserEvent)
                                                            .Include(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                                            .Where(x => x.IdEventIntendedForAtdCheckStudent == attendance.Id)
                                                            .ToListAsync(CancellationToken);
                    }

                    var attendancesPerEvent = attendances.GroupBy(x => x.EventIntendedForAttendanceStudent.EventIntendedFor.Event);

                    // get user audit trail
                    var idAuditUsers = results2.Where(x => x.Audit.UserIn != null || x.Audit.UserUp != null)
                        .SelectMany(x => new[] { x.Audit.UserIn.Id, x.Audit.UserUp?.Id })
                        .Where(x => x != null)
                        .Distinct().ToArray();
                    var auditUsers = new List<MsUser>();
                    if (idAuditUsers.Length != 0)
                    {
                        auditUsers = await _dbContext.Entity<MsUser>()
                            .Where(x => idAuditUsers.Contains(x.Id))
                            .ToListAsync(CancellationToken);
                    }

                    var isHomeroomTeacher = attendanceStudents.Any(x => x.EventIntendedForAtdPICStudents.Any(y => y.IdUser == param.IdUser && (y.Type == EventIntendedForAttendancePICStudent.Homeroom ||y.Type == EventIntendedForAttendancePICStudent.UserTeacher)));
                    var homerooms = isHomeroomTeacher ? await _dbContext.Entity<MsHomeroomTeacher>()
                                                      .Include(x => x.Homeroom)
                                                      .Where(x => x.IdBinusian == param.IdUser)
                                                      .Select(x => new { x.Homeroom.IdAcademicYear, x.IdHomeroom })
                                                      .ToListAsync() : null;

                    var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdAcademicYear, x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus})
                        .Where(x => x.IdAcademicYear == attendanceStudents.FirstOrDefault().EventIntendedFor.Event.IdAcademicYear && (x.StartDate == param.Date.Date || x.EndDate == param.Date.Date 
                            || (x.StartDate < param.Date.Date
                                ? x.EndDate != null ? (x.EndDate > param.Date.Date && x.EndDate < param.Date.Date) || x.EndDate > param.Date.Date : x.StartDate <= param.Date
                                : x.EndDate != null ? ((param.Date.Date > x.StartDate && param.Date.Date < x.EndDate) || param.Date.Date > x.EndDate) : x.StartDate <= param.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                        .ToListAsync();

                    var studentNonAktif = checkStudentStatus.Select(x => x.IdStudent).ToList();
                        
                    if (isHomeroomTeacher && homerooms != null)
                    {
                        var idStudents = attendances.SelectMany(x => x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.SelectMany(y => y.UserEvents.Select(z => z.IdUser))).Distinct();
                        
                        var homeroomStudents = await _dbContext.Entity<MsHomeroomStudent>()
                                                               .Include(x => x.Homeroom)
                                                               .Where(x => idStudents.Contains(x.IdStudent))
                                                               .Where(x => !studentNonAktif.Contains(x.IdStudent))
                                                               .Select(x => new { x.Homeroom.IdAcademicYear, x.IdStudent, x.IdHomeroom })
                                                               .ToListAsync();

                        results2.ForEach(result =>
                        {
                            var attCheckEvent = attendancesPerEvent.FirstOrDefault(x => x.Key.Id == result.Id);
                            if (attCheckEvent != null)
                            {
                                var idHomerooms = homerooms.Where(x => attCheckEvent.Key.IdAcademicYear == x.IdAcademicYear).Select(x => x.IdHomeroom);
                                var _idStudents = homeroomStudents.Where(x => attCheckEvent.Key.IdAcademicYear == x.IdAcademicYear
                                                                                    && idHomerooms.Contains(x.IdHomeroom))
                                                                  .Select(x => x.IdStudent)
                                                                  .Distinct();
                                

                                result.TotalStudent = attCheckEvent.Key.EventDetails.First().UserEvents.Count(x => _idStudents.Contains(x.IdUser));
                                result.UnsaveAbsence = result.TotalStudent - attCheckEvent.First().UserEventAttendance2s.Select(x => x.UserEvent.IdUser).Distinct().Count(x => _idStudents.Contains(x));
                                result.UnexcuseAbsence = attCheckEvent
                                    .First().UserEventAttendance2s.Where(x => x.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused).Select(x => x.UserEvent.IdUser)
                                    .Count(x => _idStudents.Contains(x));
                            }

                            if (result.Audit.UserIn != null)
                            {
                                var userIn = auditUsers.Find(x => x.Id == result.Audit.UserIn.Id);
                                result.Audit.UserIn.Name = userIn?.DisplayName;

                                if (result.Audit.UserUp != null)
                                {
                                    var userUp = auditUsers.Find(x => x.Id == result.Audit.UserUp.Id);
                                    result.Audit.UserUp.Name = userUp?.DisplayName;
                                }
                            }
                        });
                    }
                    else
                    {
                        results2.ForEach(result =>
                        {
                            var attCheckEvent = attendancesPerEvent.FirstOrDefault(x => x.Key.Id == result.Id);
                            if (attCheckEvent != null)
                            {
                                result.TotalStudent = attCheckEvent.Key.EventDetails.First().UserEvents.Where(x => !studentNonAktif.Contains(x.IdUser)).Count();
                                result.UnsaveAbsence = result.TotalStudent - attCheckEvent.First().UserEventAttendance2s.Select(x => x.UserEvent.IdUser).Distinct().Count();
                                result.UnexcuseAbsence = attCheckEvent
                                    .First().UserEventAttendance2s.Where(x => x.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused).Select(x => x.UserEvent.IdUser)
                                    .Count();
                            }

                            if (result.Audit.UserIn != null)
                            {
                                var userIn = auditUsers.Find(x => x.Id == result.Audit.UserIn.Id);
                                result.Audit.UserIn.Name = userIn?.DisplayName;

                                if (result.Audit.UserUp != null)
                                {
                                    var userUp = auditUsers.Find(x => x.Id == result.Audit.UserUp.Id);
                                    result.Audit.UserUp.Name = userUp?.DisplayName;
                                }
                            }
                        });
                    }
                }

                return Request.CreateApiResult2(results2 as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
