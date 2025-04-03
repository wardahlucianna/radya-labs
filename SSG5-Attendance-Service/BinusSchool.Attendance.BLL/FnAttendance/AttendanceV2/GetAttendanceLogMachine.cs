using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceLogMachine : FunctionsHttpSingleHandler
    {
        private readonly IServiceProvider _provider;
        private readonly IAuth _apiAuth;
        private readonly IAttendanceLog _apiAttendanceLog;

        public GetAttendanceLogMachine(IServiceProvider provider, IAttendanceLog apiAttendanceLog, IAuth apiAuth)
        {
            _provider = provider;
            _apiAttendanceLog = apiAttendanceLog;
            _apiAuth = apiAuth;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<GetAttendanceLogRequest>();

            var item = await AttendanceLogMachine(_apiAttendanceLog, _apiAuth, body);

            return Request.CreateApiResult2(item as object);
        }

        public static async Task<List<AttendanceLog>> AttendanceLogMachine (IAttendanceLog apiAttendanceLog, IAuth apiAuth, GetAttendanceLogRequest body)
        {
            var getToken = await apiAuth.GetToken();
            List<AttendanceLog> listAttendanceLogs = new List<AttendanceLog>();
            var token = "";
            if (getToken.ResultCode==200)
            {
                token = getToken.Data.Token;

                var attRequest = new GetAttendanceLogRequest
                {
                    IdSchool = body.IdSchool,
                    Year = body.Year,
                    Month = body.Month,
                    Day = body.Day,
                    StartHour = body.StartHour,
                    EndHour = body.EndHour,
                    StartMinutes = body.StartMinutes,
                    EndMinutes = body.EndMinutes,
                    ListStudent = body.ListStudent
                };

                var getAttendanceLogs = await apiAttendanceLog.GetAttendanceLogs($"Bearer {token}", attRequest);

                if (getAttendanceLogs.ResultCode == 200)
                {
                    listAttendanceLogs = getAttendanceLogs.AttendanceLogResponse;
                }
            }

            return listAttendanceLogs;
        }

    }
}
