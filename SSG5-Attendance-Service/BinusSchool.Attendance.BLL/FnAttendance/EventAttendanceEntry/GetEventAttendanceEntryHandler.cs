using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Common.Exceptions;
using Microsoft.FeatureManagement;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetEventAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetEventAttendanceEntryRequest.IdEventCheck),
            nameof(GetEventAttendanceEntryRequest.IdLevel),
            nameof(GetEventAttendanceEntryRequest.Date)
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetEventAttendanceEntryHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventAttendanceEntryRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var checkAndLevelQuery = 
                    from lvl in _dbContext.Entity<MsLevel>()
                    let chk = _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                        .Where(x => x.Id == param.IdEventCheck)
                        .Select(x => new { x.Id, x.CheckName, x.EventIntendedForAttendanceStudent.Type })
                        .FirstOrDefault()
                    where lvl.Id == param.IdLevel
                    select new { ReqLevel = new CodeWithIdVm(lvl.Id, lvl.Code, lvl.Description), ReqAttendanceCheck = chk, IdAcademicYear = lvl.IdAcademicYear };
                var checkAndLevel = await checkAndLevelQuery.FirstOrDefaultAsync(CancellationToken);
                
                if (checkAndLevel.ReqAttendanceCheck is null)
                    throw new NotFoundException("Event check is not found");
                if (checkAndLevel.ReqLevel is null)
                    throw new NotFoundException("Level is not found");

                var attendanceStudents = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                        .Include(x => x.EventIntendedFor).ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails)
                        .Include(x => x.EventIntendedForAtdPICStudents)
                        .Include(x => x.EventIntendedForAtdCheckStudents).ThenInclude(x => x.UserEventAttendance2s)
                        .Where(x=> x.EventIntendedFor.IdEvent == param.IdEvent)
                        .ToListAsync(CancellationToken);

                var isHomeroomTeacher = attendanceStudents.Any(x => x.EventIntendedForAtdPICStudents.Any(y => y.IdUser == param.IdUser && (y.Type == EventIntendedForAttendancePICStudent.Homeroom || y.Type == EventIntendedForAttendancePICStudent.UserTeacher)));
                var homerooms = isHomeroomTeacher ? await _dbContext.Entity<MsHomeroomTeacher>()
                                                  .Include(x => x.Homeroom)
                                                  .Where(x => x.IdBinusian == param.IdUser && x.Homeroom.IdAcademicYear == param.IdAcademicYear)
                                                  .Select(x => new { x.IdHomeroom, x.Homeroom.Semester })
                                                  .ToListAsync() : null;

                var attendanceEventDate = await _dbContext.Entity<TrEventDetail>()
                                    .Where(x => x.IdEvent == param.IdEvent)
                                    .ToListAsync(CancellationToken);

                var startEventDate = attendanceEventDate.Select(x => x.StartDate).Min();
                var endEventDate = attendanceEventDate.Select(x => x.EndDate).Max();

                var semester = await _dbContext.Entity<MsPeriod>()
                            .Where(x => x.StartDate <= startEventDate && x.EndDate >= endEventDate)
                            .Select(x=> x.Semester)
                            .FirstOrDefaultAsync();

                if (semester == null)
                    semester = 1;

                if (homerooms != null)
                {
                    if (homerooms.Count > 0)
                    {

                        if (string.IsNullOrEmpty(param.IdHomeroom))
                            param.IdHomeroom = homerooms.Where(x => x.Semester == semester).Select(x => x.IdHomeroom).FirstOrDefault();
                    }
                }

                var idStudens = await _dbContext.Entity<MsStudent>()
                    .Where(x 
                        => x.HomeroomStudents.Any(y => y.Homeroom.GradePathwayClassroom.GradePathway.Grade.IdLevel == param.IdLevel)
                        && (string.IsNullOrEmpty(param.IdGrade) || x.HomeroomStudents.Any(y => y.Homeroom.GradePathwayClassroom.GradePathway.IdGrade == param.IdGrade))
                        && (string.IsNullOrEmpty(param.IdHomeroom) || x.HomeroomStudents.Any(y => y.IdHomeroom == param.IdHomeroom))
                        && (string.IsNullOrEmpty(param.IdSubject) || x.HomeroomStudents.Any(y => y.Homeroom.HomeroomPathways.Any(z => z.LessonPathways.Any(za => za.Lesson.IdSubject == param.IdSubject)))))
                    .Select(x => x.Id)
                    .ToListAsync(CancellationToken);

                var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear})
                .Where(x => x.ActiveStatus == false && x.CurrentStatus == "A" && x.IdAcademicYear == checkAndLevel.IdAcademicYear && (x.StartDate == param.Date.Date || x.EndDate == param.Date.Date 
                    || (x.StartDate < param.Date.Date
                        ? x.EndDate != null ? (x.EndDate > param.Date.Date && x.EndDate < param.Date.Date) || x.EndDate > param.Date.Date : x.StartDate <= param.Date
                        : x.EndDate != null ? ((param.Date.Date > x.StartDate && param.Date.Date < x.EndDate) || param.Date.Date > x.EndDate) : x.StartDate <= param.Date)))
                .ToListAsync();

                var userEvents = await _dbContext.Entity<TrUserEvent>()
                    .Where(x 
                        => idStudens.Contains(x.IdUser)
                        && x.EventDetail.Event.EventIntendedFor
                            .Where(y => y.IntendedFor == RoleConstant.Student)
                            .Any(y => y.EventIntendedForAttendanceStudents.Any(z => z.EventIntendedForAtdCheckStudents.Any(a => a.Id == param.IdEventCheck))))
                    .Select(x => x.UserEventAttendance2s.Any(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck)
                        ?
                        new EventAttendanceEntryStudent
                        {
                            Id = x.IdUser,
                            Name = x.User.DisplayName,
                            IdUserEvent = x.Id,
                            Attendance = new AttendanceEntryItem
                            {
                                IdAttendanceMapAttendance = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).IdAttendanceMappingAttendance,
                                Id = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).AttendanceMappingAttendance.IdAttendance,
                                Code = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).AttendanceMappingAttendance.Attendance.Code,
                                Description = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).AttendanceMappingAttendance.Attendance.Description
                            },
                            Workhabits = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).UserEventAttendanceWorkhabits.Select(z => new AttendanceEntryItemWorkhabit
                            {
                                Id = z.IdMappingAttendanceWorkhabit,
                                Code = z.MappingAttendanceWorkhabit.Workhabit.Code,
                                Description = z.MappingAttendanceWorkhabit.Workhabit.Description,
                                IsTick = true
                            }),
                            Late = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).LateTime,
                            File = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).FileEvidence,
                            Note = x.UserEventAttendance2s.OrderByDescending(y => y.DateIn).First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).Notes,
                            IsSubmitted = true
                        }
                        : new EventAttendanceEntryStudent
                        {
                            Id = x.IdUser,
                            Name = x.User.DisplayName,
                            IdUserEvent = x.Id,
                            IsSubmitted = false
                        })
                    .ToListAsync(CancellationToken);

                if(checkStudentStatus != null)
                {
                    userEvents = userEvents.Where(x => !checkStudentStatus.Select(z=>z.IdStudent).ToList().Contains(x.Id)).ToList();
                }
                
                var result2 = new EventAttendanceEntryResult
                {
                    EventCheck = new ItemValueVm(checkAndLevel.ReqAttendanceCheck.Id, checkAndLevel.ReqAttendanceCheck.CheckName),
                    Level = checkAndLevel.ReqLevel,
                    Summary = new EventAttendanceEntrySummary
                    {
                        TotalStudent = userEvents.Count,
                        Submitted = userEvents.Count(x => x.IsSubmitted)
                    },
                    Entries = userEvents.Where(x => param.IsSubmitted.HasValue ? x.IsSubmitted == param.IsSubmitted.Value : true),
                    AttendanceType = checkAndLevel.ReqAttendanceCheck.Type
                };

                return Request.CreateApiResult2(result2 as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
