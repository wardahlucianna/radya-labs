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
    public class GetAttendanceSummaryDetailLevelWidgeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryDetailLevelWidgeHandler(IAttendanceDbContext dbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _attendanceSummaryTerm = AttendanceSummaryTerm;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailLevelRequest>();
            param.Return = CollectionType.Lov;
            param.GetAll = true;

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var listAttendaceSummaryLevel = _attendanceSummaryTerm.GetAttendanceSummaryDetailLevel(param).Result.Payload;

            var GetMappingAttendance = redisMappingAttendance.FirstOrDefault();

            GetAttendanceSummaryDetailLevelWidgeResult Items = new GetAttendanceSummaryDetailLevelWidgeResult
            {
                Unsubmited = listAttendaceSummaryLevel.Select(e=>e.Unsubmited).Sum(),
                Pending = listAttendaceSummaryLevel.Select(e => e.Pending).Sum(),
                IsShowPending = GetMappingAttendance.IsNeedValidation,
            };

            return Request.CreateApiResult2(Items as object);
        }


    }
}
