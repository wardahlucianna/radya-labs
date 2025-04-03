using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class SendAttendanceSumamryEmailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public SendAttendanceSumamryEmailHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<SendAttendanceSumamryEmailRequest>();

            if (KeyValues.ContainsKey("EmailDailyRecap"))
            {
                KeyValues.Remove("EmailDailyRecap");
            }
            KeyValues.Add("EmailDailyRecap", param);

            if (param.IdScenario == "EHN9")
                EHN9Notification(KeyValues);
            else if (param.IdScenario == "EHN10")
                EHN10Notification(KeyValues);

            return Request.CreateApiResult2();
        }

        public static string EHN9Notification(IDictionary<string, object> KeyValues)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailDailyRecap").Value;
            var EmailDailyRecap = JsonConvert.DeserializeObject<SendAttendanceSumamryEmailRequest>(JsonConvert.SerializeObject(Object));
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(EmailDailyRecap.IdSchool, "EHN9")
                {
                    IdRecipients = new List<string>{ EmailDailyRecap.IdUser },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string EHN10Notification(IDictionary<string, object> KeyValues)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailDailyRecap").Value;
            var EmailDailyRecap = JsonConvert.DeserializeObject<SendAttendanceSumamryEmailRequest>(JsonConvert.SerializeObject(Object));
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(EmailDailyRecap.IdSchool, "EHN10")
                {
                    IdRecipients = new List<string> { EmailDailyRecap.IdUser },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
