using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDayClassIdAndSessionHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IAttendanceSummary _apiAttendanceSummary;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDayClassIdAndSessionHandler(IAttendanceDbContext dbContext, IMachineDateTime DateTime, IAttendanceSummary ApiAttendanceSummary, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _dateTime = DateTime;
            _apiAttendanceSummary = ApiAttendanceSummary;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDayClassIdAndSessionRequest>();

            #region get id lesson per user login
            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdClassroom = param.IdClassroom
            };

            var listIdLesson = await AttendanceSummaryRedisCacheHandler.GetLessonByUser(_dbContext, CancellationToken, filterIdHomerooms, _redisCache);
            #endregion

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisScheduleLesson = await AttendanceSummaryRedisCacheHandler.GetScheduleLesson(paramRedis, _redisCache, _dbContext, CancellationToken, _dateTime.ServerTime.Date);
            #endregion

            var listScheduleLesson = redisScheduleLesson
                                      .Where(x => listIdLesson.Contains(x.IdLesson))
                                      .GroupBy(e => new 
                                      {
                                          classId = e.ClassID,
                                          idSession = e.Session.Id,
                                          sessionName = e.Session.Name,
                                          sessionID = e.Session.SessionID
                                      })
                                      .Select(e=>e.Key)
                                      .ToList();

            var listIdClassId = listScheduleLesson.Select(e => e.classId).Distinct().ToList();

            var items = listIdClassId
                        .Select(e => new GetAttendanceSummaryDayClassIdAndSessionResult
                        {
                            ClassId = e,
                            Session = listScheduleLesson
                                        .Where(x => x.classId == e)
                                        .Select(x => new ItemValueVm
                                        {
                                            Id = x.idSession,
                                            Description = x.sessionID
                                        })
                                        .ToList()
                        }).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
