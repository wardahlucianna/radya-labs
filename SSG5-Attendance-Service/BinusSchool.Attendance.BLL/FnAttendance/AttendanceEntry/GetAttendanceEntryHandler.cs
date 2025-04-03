using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using EasyCaching.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry
{
    public class GetAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;
        private readonly IServiceProvider _provider;

        public GetAttendanceEntryHandler(IAttendanceDbContext dbContext, GetMapAttendanceDetailHandler mapAttendanceHandler, IServiceProvider provider)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceEntryRequest>(
                nameof(GetAttendanceEntryRequest.Date),
                nameof(GetAttendanceEntryRequest.CurrentPosition));

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.IsGenerated
                && EF.Functions.DateDiffDay(x.ScheduleDate, param.Date) == 0);

            //if (param.Status.HasValue)
            //{
            //    predicate = predicate.And(x => param.Status.Value == AttendanceEntryStatus.Unsubmitted
            //        ? !x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending) &&
            //          !x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Submitted)
            //        : x.AttendanceEntries.Any(y => y.Status == param.Status));
            //}

            string idLevel = null, idGrade = null;
            if (param.IdHomeroom is null && param.ClassId is null)
                throw new BadRequestException("at least data homeroom / class id must be inputted");

            if (param.IdHomeroom != null)
            {

                var levelGrade = await _dbContext.Entity<MsHomeroom>()
                                          .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                          .Where(x => x.Id == param.IdHomeroom)
                                          .Select(x => new
                                          {
                                              x.GradePathwayClassroom.GradePathway.Grade.IdLevel,
                                              x.GradePathwayClassroom.GradePathway.IdGrade
                                          })
                                          .FirstOrDefaultAsync(CancellationToken);

                idLevel = levelGrade?.IdLevel;
                idGrade = levelGrade?.IdGrade;
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);
            }
            else if (param.ClassId != null)
            {
                var levelAndGrade = await (from _gsl in _dbContext.Entity<TrGeneratedScheduleLesson>()
                                           join _l in _dbContext.Entity<MsLesson>() on _gsl.IdLesson equals _l.Id
                                           join _g in _dbContext.Entity<MsGrade>() on _l.IdGrade equals _g.Id
                                           join _lv in _dbContext.Entity<MsLevel>() on _g.IdLevel equals _lv.Id
                                           join _ac in _dbContext.Entity<MsAcademicYear>() on _lv.IdAcademicYear equals _ac.Id
                                           where
                                                _gsl.ClassID == param.ClassId
                                                && _gsl.ScheduleDate.Date == param.Date
                                                && _ac.IdSchool == param.IdSchool
                                           select new
                                           {
                                               _g.IdLevel,
                                               IdGrade = _g.Id
                                           }
                           ).FirstOrDefaultAsync(CancellationToken);

                idLevel = levelAndGrade?.IdLevel;
                idGrade = levelAndGrade?.IdGrade;
            }

            if (idLevel is null)
                throw new NotFoundException("data homeroom / class id is not found");

            var mapAttendance = await _mapAttendanceHandler.GetMapAttendanceDetail(idLevel, CancellationToken);
            if (mapAttendance is null)
            {
                var levelDesc = await _dbContext.Entity<MsLevel>().Where(x => x.Id == idLevel)
                    .Select(x => x.Description).FirstOrDefaultAsync(CancellationToken);
                throw new BadRequestException($"Mapping attendance for level {levelDesc ?? idLevel} is not available.");
            }
            if (mapAttendance.Term == AbsentTerm.Session)
            {
                param = Request.ValidateParams<GetAttendanceEntryRequest>(
                    nameof(GetAttendanceEntryRequest.ClassId), nameof(GetAttendanceEntryRequest.IdSession));
                predicate = predicate.And(x => x.ClassID == param.ClassId && x.IdSession == param.IdSession);
            }
            else if (mapAttendance.Term == AbsentTerm.Day)
            {
                if (param.CurrentPosition == PositionConstant.SubjectTeacher)
                {
                    param = Request.ValidateParams<GetAttendanceEntryRequest>(
                    nameof(GetAttendanceEntryRequest.ClassId), nameof(GetAttendanceEntryRequest.IdSession));

                    predicate = predicate.And(x => x.ClassID == param.ClassId && x.IdSession == param.IdSession);
                }
                else
                {
                    if (!string.IsNullOrEmpty(param.IdSession))
                        predicate = predicate.And(x => x.IdSession == param.IdSession);
                    else if (!string.IsNullOrEmpty(param.ClassId))
                        predicate = predicate.And(x => x.ClassID == param.ClassId);
                }
            }

            var query = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                .Include(x => x.AttendanceEntries)
                    .ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                .Include(x => x.AttendanceEntries)
                    .ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit).ThenInclude(x => x.Workhabit)
                .OrderBy(x => x.StartTime)
                .Where(predicate)
                .ToListAsync(CancellationToken);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus})
                .Where(x => (x.StartDate == param.Date.Date || x.EndDate == param.Date.Date 
                    || (x.StartDate < param.Date.Date
                        ? x.EndDate != null ? (x.EndDate > param.Date.Date && x.EndDate < param.Date.Date) || x.EndDate > param.Date.Date : x.StartDate <= param.Date
                        : x.EndDate != null ? ((param.Date.Date > x.StartDate && param.Date.Date < x.EndDate) || param.Date.Date > x.EndDate) : x.StartDate <= param.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            if(checkStudentStatus != null)
            {
                query = query.Where(x => !checkStudentStatus.Select(z=>z.IdStudent).ToList().Contains(x.GeneratedScheduleStudent.IdStudent)).ToList();
            }

            if (query.Count != 0)
            {
                query = query.OrderByDescending(x => x.AttendanceEntries.Count == 0 ? 0 : 1).ToList();
                //var selectTeacher = param.IdUser;

                if (mapAttendance.Term == AbsentTerm.Session)
                {
                    //if (query.Where(x => x.IdUser == param.IdUser).Count() > 0)
                    //{
                    //    var selectTeacher = param.IdUser;
                    //    query = query.Where(x => x.IdUser == selectTeacher).ToList();
                    //}
                    //else
                    //{
                    //    var listTeacher = query.GroupBy(x => x.IdUser).Select(x => x.Key).ToList();

                    //    query = query.Where(x => listTeacher.Contains(x.IdUser)).ToList();
                    //}
                    var selectTeacher = query.First().IdUser;

                    query = query.Where(x => x.IdUser == selectTeacher).ToList();
                }
                else
                {
                    var listTeacher = query.GroupBy(x => x.IdUser).Select(x => x.Key).ToList();

                    query = query.Where(x => listTeacher.Contains(x.IdUser)).ToList();
                }
            }

            if (query.Count == 0)
                throw new BadRequestException("Attendance entry not available");

            if (param.Status.HasValue)
            {
                if (param.Status.Value == AttendanceEntryStatus.Unsubmitted)
                {
                    var listUnsubmitted = new List<TrGeneratedScheduleLesson>();
                    var query2 = query.Where(x => !x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending) &&
                      !x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Submitted)).ToList();
                    
                    foreach (var item in query2)
                    {
                        if (mapAttendance.Term == AbsentTerm.Day)
                        {
                            if (query.Any(x => x.ScheduleDate == item.ScheduleDate && x.IdHomeroom == item.IdHomeroom &&
                                x.IdGeneratedScheduleStudent == item.IdGeneratedScheduleStudent) == false)
                            {
                                listUnsubmitted.Add(item);
                            }
                        }
                        else
                        {
                            if (query.Any(x => x.ScheduleDate == item.ScheduleDate && x.IdHomeroom == item.IdHomeroom && x.IdSession == item.IdSession &&
                                x.IdGeneratedScheduleStudent == item.IdGeneratedScheduleStudent && x.ClassID == item.ClassID && x.IdLesson == item.IdLesson) == false)
                            {
                                listUnsubmitted.Add(item);
                            }
                        }
                    }
                    query = listUnsubmitted;
                }
                else
                {
                    query = query.Where(x => x.AttendanceEntries.Any(y => y.Status == param.Status)).ToList();                    
                }
            }

            var entries = query
                .Select(x => x.AttendanceEntries.Count != 0
                    ? new AttendanceEntryStudent
                    {
                        Id = x.GeneratedScheduleStudent.IdStudent,
                        IdGeneratedScheduleLesson = x.Id,
                        IdSession = x.IdSession,
                        Name = NameUtil.GenerateFullName(
                            x.GeneratedScheduleStudent.Student.FirstName,
                            x.GeneratedScheduleStudent.Student.MiddleName,
                            x.GeneratedScheduleStudent.Student.LastName),
                        Attendance = new AttendanceEntryItem
                        {
                            IdAttendanceMapAttendance = x.AttendanceEntries.First().IdAttendanceMappingAttendance,
                            Id = x.AttendanceEntries.First().AttendanceMappingAttendance.IdAttendance,
                            Code = x.AttendanceEntries.First().AttendanceMappingAttendance.Attendance.Code,
                            Description = x.AttendanceEntries.First().AttendanceMappingAttendance.Attendance.Description,
                            IsFromAttendanceAdministration = x.AttendanceEntries.First().IsFromAttendanceAdministration
                        },
                        Workhabits = x.AttendanceEntries.First().AttendanceEntryWorkhabits.Select(y => new AttendanceEntryItemWorkhabit
                        {
                            Id = y.IdMappingAttendanceWorkhabit,
                            Code = y.MappingAttendanceWorkhabit.Workhabit.Code,
                            Description = y.MappingAttendanceWorkhabit.Workhabit.Description,
                            IsTick = true
                        }),
                        Late = x.AttendanceEntries.First().LateTime,
                        File = x.AttendanceEntries.First().FileEvidence,
                        Note = x.AttendanceEntries.First().Notes,
                        Status = x.AttendanceEntries.First().Status,
                        PositionIn = x.AttendanceEntries.First().PositionIn
                    }
                    : new AttendanceEntryStudent
                    {
                        Id = x.GeneratedScheduleStudent.IdStudent,
                        IdGeneratedScheduleLesson = x.Id,
                        IdSession = x.IdSession,
                        Name = NameUtil.GenerateFullName(
                            x.GeneratedScheduleStudent.Student.FirstName,
                            x.GeneratedScheduleStudent.Student.MiddleName,
                            x.GeneratedScheduleStudent.Student.LastName),
                        Status = AttendanceEntryStatus.Unsubmitted,
                        PositionIn = null
                    })
                .If(mapAttendance.Term == AbsentTerm.Day, x => x.Distinct(new UniqueIdComparer<AttendanceEntryStudent>()))
                .OrderBy(x => x.Name)
                .ToList();

            // school Simprug can fill attendance with tapping id card
            //if (mapAttendance.School.Id == "1" && mapAttendance.Term == AbsentTerm.Day)
            //{
            //    var (isSuccess, bearerToken) = await TryGetAndCacheBinusianToken();

            //    if (isSuccess)
            //    {
            //        // get school hour for requested grade
            //        var session = await _dbContext.Entity<MsGradePathway>()
            //            .Where(x => x.IdGrade == idGrade)
            //            .Select(x => new
            //            {
            //                Min = x.Sessions.Min(y => y.StartTime),
            //                Max = x.Sessions.Max(y => y.EndTime)
            //            })
            //            .FirstOrDefaultAsync(CancellationToken);

            //        var attendanceLogs = await GetAttendanceLogs(bearerToken,
            //            mapAttendance.School.Id, param.Date, session.Min, session.Max,
            //            entries.Select(x => x.Id));

            //        if (attendanceLogs != null)
            //        {
            //            foreach (var attendanceLog in attendanceLogs)
            //            {
            //                var currentEntry = entries.Find(x => x.Id == attendanceLog.BinusianID);

            //                // skip tapping check if entry already submitted
            //                if (currentEntry!.Status == AttendanceEntryStatus.Submitted)
            //                    continue;

            //                // check start time with tapping time
            //                if (attendanceLog.DetectedDate.TimeOfDay > session.Min)
            //                    currentEntry.Late = attendanceLog.DetectedDate.TimeOfDay - session.Min;
            //            }
            //        }
            //    }

            //}

            var level = await _dbContext.Entity<MsLevel>().FindAsync(new[] { idLevel }, CancellationToken);
            var homeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(x => x.Homeroom)
                .Include(x => x.TeacherPosition).ThenInclude(x=> x.LtPosition)
                .Where(x => x.Homeroom.Id == param.IdHomeroom && x.TeacherPosition.LtPosition.Code == "CA")
                .Select(x => x.IdBinusian).FirstOrDefaultAsync(CancellationToken);

            NameValueVm teacher = null;

            if (homeroomTeacher != null)
            {
                teacher = await _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == homeroomTeacher)
                   .Select(x => new NameValueVm
                   {
                       Id = x.IdBinusian,
                       Name = String.Format("{0} {1}", x.FirstName, x.LastName)
                   }).FirstOrDefaultAsync(CancellationToken);
            }
            var dataMappingLevelCheckboxAttendance = await _dbContext.Entity<MsMappingAttendance>().Where(x => x.IdLevel == idLevel).FirstOrDefaultAsync(CancellationToken);
            var idHomeroom = query.FirstOrDefault()?.IdHomeroom;
            var homeroomData = await _dbContext.Entity<MsHomeroom>()
                                               .Include(x => x.Grade)
                                               .Where(x => x.Id == idHomeroom)
                                               .FirstOrDefaultAsync(CancellationToken);

            var result = new GetAttendanceEntryResult
            {
                Id = query.FirstOrDefault()?.IdHomeroom,
                Code = query.FirstOrDefault()?.HomeroomName,
                Level = new CodeWithIdVm(level.Id, level.Code, level.Description),
                Grade = homeroomData != null ? new CodeWithIdVm(homeroomData.Grade.Id, homeroomData.Grade.Code, homeroomData.Grade.Description) : null,
                Semester = homeroomData != null ? homeroomData.Semester : 0,
                UsingCheckboxAttendance = dataMappingLevelCheckboxAttendance != null ? dataMappingLevelCheckboxAttendance.UsingCheckboxAttendance : true,
                RenderAttendance = dataMappingLevelCheckboxAttendance != null ? dataMappingLevelCheckboxAttendance.RenderAttendance : RenderAttendance.Dropdown,
                NeedValidation = mapAttendance.NeedValidation,
                Teacher = teacher,
                Date = param.Date,
                Session = mapAttendance.Term == AbsentTerm.Session
                    ? query.FirstOrDefault()?.SessionID
                    : null,
                Summary = new AttendanceEntrySummary
                {
                    TotalStudent = entries.Count,
                    Pending = entries.Count(x => x.Status == AttendanceEntryStatus.Pending),
                    Submitted = entries.Count(x => x.Status == AttendanceEntryStatus.Submitted)
                },
                Entries = entries
            };

            return Request.CreateApiResult2(result as object);
        }

        private async Task<(bool isSuccess, string bearerToken)> TryGetAndCacheBinusianToken()
        {
            var easyCache = _provider.GetRequiredService<IEasyCachingProvider>();
            var binusianToken = default(string);

            // return from cache if exist or not expired
            if (easyCache.TryGetCacheValue(nameof(binusianToken), out binusianToken))
                return (true, binusianToken);

            // otherwise fetch from api then cache it
            var binusianAuthService = _provider.GetRequiredService<IAuth>();
            var result = await binusianAuthService.GetToken();

            if (result?.ResultCode == 200)
            {
                await easyCache.SetAsync(nameof(binusianToken), result.Data.Token, TimeSpan.FromMinutes(result.Data.Duration - 1));
                return (true, result.Data.Token);
            }

            return (false, null);
        }

        private async Task<IEnumerable<AttendanceLog>> GetAttendanceLogs(string token, string idSchool, DateTime date, TimeSpan min, TimeSpan max, IEnumerable<string> idStudents)
        {
            var attendanceLogService = _provider.GetRequiredService<IAttendanceLog>();
            var attRequest = new GetAttendanceLogRequest
            {
                IdSchool = idSchool,
                Year = date.Year,
                Month = date.Month,
                Day = date.Day,
                StartHour = min.Hours,
                EndHour = max.Hours,
                StartMinutes = min.Minutes,
                EndMinutes = max.Minutes,
                ListStudent = idStudents
            };
            var attendanceLogResult = await attendanceLogService.GetAttendanceLogs($"Bearer {token}", attRequest);

            if (attendanceLogResult?.ResultCode == 200)
            {
                return attendanceLogResult.AttendanceLogResponse;
            }

            return null;
        }
    }
}
