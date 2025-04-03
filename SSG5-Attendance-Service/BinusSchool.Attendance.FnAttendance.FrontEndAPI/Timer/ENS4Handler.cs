using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Model.Enums;
using FluentEmail.Core;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using Microsoft.Extensions.Azure;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Drawing.Charts;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Attendance.FnAttendance.AttendanceSummary.Notification;

namespace BinusSchool.Attendance.FnAttendance.Timer
{
    public class ENS4Handler
    {
        private readonly ILogger<ENS4Handler> _logger;
        private readonly IServiceProvider _provider;
        private readonly IMachineDateTime _dateTime;
        private readonly IAttendanceV2 _attendanceService;
        private readonly IAttendanceDbContext _dbContex;
        private readonly IRolePosition _rolePositionService;
        protected IDictionary<string, object> KeyValues =  new Dictionary<string, object>();

        public ENS4Handler(
            ILogger<ENS4Handler> logger,
            IServiceProvider provider,
            IAttendanceV2 attendanceService,
            IAttendanceDbContext dbContex,
            IMachineDateTime dateTime,
            IRolePosition rolePositionService)
        {
            _logger = logger;
            _attendanceService = attendanceService;
            _provider = provider;
            _dateTime = dateTime;
            _dbContex = dbContex;
            _rolePositionService = rolePositionService;
        }

        [FunctionName(nameof(ENS4))]
        public async Task ENS4([TimerTrigger(AttendanceTimeConstant.ENS4ConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;

            using (var scope = _provider.CreateScope())
            {
                for (var i = 0; i < schools.Count; i++)
                {
                    _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ENS4), schools[i]);
                    var handlerV2 = scope.ServiceProvider.GetService<ENS4Notification>();
                    await handlerV2.Execute(schools[i], null, null, cancellationToken);
                }
            }
        }
    }
}
