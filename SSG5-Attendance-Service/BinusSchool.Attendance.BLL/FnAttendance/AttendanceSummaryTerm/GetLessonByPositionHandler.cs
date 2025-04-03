using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetLessonByPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        public GetLessonByPositionHandler(IAttendanceDbContext AttendanceDbContext, IRedisCache redisCache)
        {
            _dbContext = AttendanceDbContext;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryUnsubmitedRequest>();

            #region get id lesson per user login
            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdHomeroom = param.IdHomeroom,
                IdSubject = param.IdSubject,
                IdClassroom = param.IdClassroom
            };

            var listIdLesson = await AttendanceSummaryRedisCacheHandler.GetLessonByUser(_dbContext, CancellationToken, filterIdHomerooms, _redisCache);
            #endregion

            return Request.CreateApiResult2(listIdLesson as object);
        }

    }
}
