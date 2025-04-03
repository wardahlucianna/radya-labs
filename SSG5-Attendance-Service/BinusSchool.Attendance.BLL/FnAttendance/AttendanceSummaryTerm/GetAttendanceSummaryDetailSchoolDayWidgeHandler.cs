using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSchoolDayWidgeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDetailSchoolDayWidgeHandler(IAttendanceDbContext dbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _attendanceSummaryTerm = AttendanceSummaryTerm;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailSchoolDayRequest>();
            param.Return = CollectionType.Lov;
            param.GetAll = true;

            var getAttendaceSummarySchoolDay = await _attendanceSummaryTerm.GetAttendanceSummaryDetailSchoolDay(param);

            if (!getAttendaceSummarySchoolDay.IsSuccess)
                return Request.CreateApiResult2();

            var listAttendaceSummarySchoolDay = getAttendaceSummarySchoolDay.Payload.SelectMany(e => e.DataAttendanceAndWorkhabit).ToList();

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var GetMappingAttendance = redisMappingAttendance.FirstOrDefault();

            GetAttendanceSummaryDetailSchoolDayWidgeResult Items = default;

            if (redisMappingAttendance.Count == 0)
                Request.CreateApiResult2(Items as object);

            Items = new GetAttendanceSummaryDetailSchoolDayWidgeResult
            {
                Unsubmited = listAttendaceSummarySchoolDay.Where(e => e.Description== "Unsubmited" && e.Type=="Attendance").Select(e=>e.Total).Sum(),
                Pending = listAttendaceSummarySchoolDay.Where(e => e.Description == "Pending" && e.Type == "Attendance").Select(e => e.Total).Sum(),
                Present = listAttendaceSummarySchoolDay.Where(e => e.Code == "PR" && e.Type == "Attendance").Select(e => e.Total).Sum(),
                Late = listAttendaceSummarySchoolDay.Where(e => e.Code == "LT" && e.Type == "Attendance").Select(e => e.Total).Sum(),
                UnexcusedAbsence = listAttendaceSummarySchoolDay.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused && e.Type == "Attendance")
                                    .Select(e => e.Total).Sum(),
                ExcusedAbsence = listAttendaceSummarySchoolDay.Where(e => e.AbsenceCategory == AbsenceCategory.Excused && e.Type == "Attendance")
                                    .Select(e => e.Total).Sum(),
                IsShowPending = GetMappingAttendance.IsNeedValidation,
            };

            return Request.CreateApiResult2(Items as object);
        }


    }
}
