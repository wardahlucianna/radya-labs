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
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailWidgeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryDetailWidgeHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailRequest>();

            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdClassroom = param.IdClassroom,
                IdGrade = param.IdGrade,
                IdLevel = param.IdLevel,
                Semester = param.Semester
            };

            var IdHomerooms = await GetAttendanceSummaryHandler.GetHomeroomByUser(_dbContext, CancellationToken, filterIdHomerooms);

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisAttendance = await AttendanceSummaryRedisCacheHandler.GetAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var GetExcusedAbsen = redisAttendance.Where(e => e.AbsenceCategory == AbsenceCategory.Excused).Select(e => e.Id).ToList();
            var GetUnexcusedAbsen = redisAttendance.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused).Select(e => e.Id).ToList();
            var IdPresent = redisAttendance.Where(e => e.Code == "PR").Select(e => e.Id).FirstOrDefault();
            var IdLate = redisAttendance.Where(e => e.Code == "LT").Select(e => e.Id).FirstOrDefault();

            var LastUpdate = await _dbContext.Entity<TrAttendanceSummaryLog>()
               .Where(x => x.IsDone)
               .OrderByDescending(e => e.StartDate)
               .Select(e => e.StartDate)
               .FirstOrDefaultAsync(CancellationToken);

            var GetMappingAttendance = redisMappingAttendance.FirstOrDefault();

            var listIdStudentActive = redisStudentStatus
                                       .Where(x => x.StartDate.Date <= _datetime.ServerTime.Date
                                              && x.EndDate.Date >= _datetime.ServerTime.Date)
                                       .Select(e => e.IdStudent)
                                       .ToList();

            var Query = _dbContext.Entity<TrAttendanceSummaryTerm>()
                .Include(e => e.Level)
                .Include(e => e.AcademicYear)
                .Include(e => e.Grade)
                .Include(e => e.Student)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Where(x => IdHomerooms.Contains(x.IdHomeroom) && x.IdAcademicYear == param.IdAcademicYear 
                            && x.IdLevel == param.IdLevel 
                            && listIdStudentActive.Contains(x.IdStudent));

            if (!string.IsNullOrEmpty(param.IdGrade))
                Query = Query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdPeriod))
                Query = Query.Where(x => x.IdPeriod == param.IdPeriod);
            if (!string.IsNullOrEmpty(param.IdClassroom))
                Query = Query.Where(x => x.Homeroom.GradePathwayClassroom.IdClassroom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                Query = Query.Where(x => x.Homeroom.Semester == param.Semester);

            var DataAttendanceSummaryTerm = await Query.ToListAsync(CancellationToken);

            var items = new GetAttendanceSummaryDetailWidgeResult
            {
                TotalStudent = DataAttendanceSummaryTerm.Select(e => e.IdStudent).Distinct().Count(),
                Pending = DataAttendanceSummaryTerm
                            .Where(e=>e.AttendanceWorkhabitName== SummaryTermConstant.DefaultPendingName && e.AttendanceWorkhabitType== TrAttendanceSummaryTermType.AttendanceStatus)
                            .Sum(e=>e.Total),
                Present = DataAttendanceSummaryTerm
                            .Where(e => e.IdAttendanceWorkhabit==IdPresent)
                            .Sum(e => e.Total),
                Late = DataAttendanceSummaryTerm
                            .Where(e => e.IdAttendanceWorkhabit == IdLate)
                            .Sum(e => e.Total),
                ExcusedAbsence = DataAttendanceSummaryTerm
                                    .Where(x => GetExcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                        && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                    .Select(x => x.Total).Sum(),
                UnxcusedAbsence = DataAttendanceSummaryTerm
                                    .Where(x => GetUnexcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                        && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                    .Select(x => x.Total).Sum(),
                Unsubmited = DataAttendanceSummaryTerm
                            .Where(e => e.AttendanceWorkhabitName == SummaryTermConstant.DefaultUnsubmittedName && e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus)
                            .Sum(e => e.Total),
                IsUseDueToLateness = GetMappingAttendance.IsUseDueToLateness,
                IsEaGroup = !redisAttendance.Where(e=>e.ExcusedAbsenceCategory!=null).Any(),
                LastUpdate = LastUpdate,
                IsNeedValidation = GetMappingAttendance.IsNeedValidation,
                IsUseWorkhabit = GetMappingAttendance.IsUseWorkhabit,
                AbsentTerm = GetMappingAttendance.AbsentTerms
            };
            
            return Request.CreateApiResult2(items as object);
        }


    }
}
