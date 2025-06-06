﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Attendance.FnAttendance.Timer
{
    public class ENS2Handler
    {
        private readonly ILogger<ENS2Handler> _logger;
        private readonly IServiceProvider _provider;
        private readonly IMachineDateTime _dateTime;
        private readonly IAttendanceV2 _attendanceService;
        private readonly IAttendanceDbContext _dbContex;
        private readonly IRolePosition _rolePositionService;
        protected IDictionary<string, object> KeyValues = new Dictionary<string, object>();

        public ENS2Handler(
            ILogger<ENS2Handler> logger,
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

        [FunctionName(nameof(ENS2))]
        public async Task ENS2([TimerTrigger(AttendanceTimeConstant.ENS2ConstantTime
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
                    _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ENS2), schools[i]);
                    var handlerV2 = scope.ServiceProvider.GetService<ENS2Notification>();
                    await handlerV2.Execute(schools[i], null, null, cancellationToken);
                }
            }
        }
    }
}
