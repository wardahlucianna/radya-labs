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
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailUnexcusedExcusedSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        public GetAttendanceSummaryDetailUnexcusedExcusedSummaryHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache, IAttendanceSummaryTerm AttendanceSummaryTerm)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
            _attendanceSummaryTerm = AttendanceSummaryTerm;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailUnexcusedExcusedRequest>();

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisAttendance = await AttendanceSummaryRedisCacheHandler.GetAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            param.Return = CollectionType.Lov;
            param.GetAll = true;
            var listAttendaceSummaryUnexcusedExcused = _attendanceSummaryTerm.GetAttendanceSummaryDetailUnexcusedExcused(param).Result.Payload.ToList();

            var listAbsenceCategory = redisAttendance.Select(e => e.AbsenceCategory).Distinct().ToList();
            List<GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult> items = new List<GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult>();
            if (listAbsenceCategory.Any())
            {
                foreach (var absenceCategory in listAbsenceCategory)
                {
                    var listAttendanceByAbsenceCategory = redisAttendance.Where(e => e.AbsenceCategory == absenceCategory).ToList();

                    items.Add(new GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult
                    {
                        AbsenceCategory = absenceCategory,
                        Attendance = listAttendanceByAbsenceCategory
                                        .Select(e => new GetAttendance
                                        {
                                            Id = e.Id,
                                            Description = e.Description,
                                            Total = listAttendaceSummaryUnexcusedExcused.Where(f => f.IdAttendance == e.Id).Count(),
                                        }).ToList(),
                    });
                }
            }
            else
            {
                items.Add(new GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult
                {
                    AbsenceCategory = null,
                    Attendance = redisAttendance
                                    .Select(e => new GetAttendance
                                    {
                                        Id = e.Id,
                                        Description = e.Description,
                                        Total = listAttendaceSummaryUnexcusedExcused.Where(f => f.IdAttendance == e.Id).Count(),
                                    }).ToList(),
                });
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
