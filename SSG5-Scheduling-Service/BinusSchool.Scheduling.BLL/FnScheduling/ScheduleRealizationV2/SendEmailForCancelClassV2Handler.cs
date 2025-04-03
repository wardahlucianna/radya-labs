using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{

    public class SendEmailForCancelClassV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SendEmailForCancelClassV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SendEmailForCancelClassV2Request, SendEmailForCancelClassV2Validator>();

            List<string> IdUsers = new List<string>();

            foreach(var dataBody in body.DataScheduleRealizations)
            {
                IdUsers.AddRange(dataBody.IdUserTeacher);
                IdUsers.AddRange(dataBody.IdUserSubtituteTeacher);

                var EmailScheduleRealization = new GetEmailScheduleRealizationV2Result
                {
                    Ids = dataBody.Ids,
                    Date = dataBody.Date != null ? dataBody.Date : null,
                    SessionID = dataBody.SessionID,
                    SessionStartTime = dataBody.SessionStartTime,
                    SessionEndTime = dataBody.SessionEndTime,
                    ClassID = dataBody.ClassID,
                    IdUserTeacher = dataBody.IdUserTeacher,
                    IdUserSubtituteTeacher = dataBody.IdUserSubtituteTeacher,
                    IdUsers = IdUsers,
                    IdRegularVenue = dataBody.IdRegularVenue,
                    IdChangeVenue = dataBody.IdChangeVenue
                };

                if (KeyValues.ContainsKey("EmailScheduleRealization"))
                {
                    KeyValues.Remove("EmailScheduleRealization");
                }
                KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);

                var Notification = SR5Notification(KeyValues, AuthInfo);
            }

            return Request.CreateApiResult2();
        }

        public static string SR5Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR5")
                {
                    IdRecipients = EmailScheduleRealization.IdUsers.Select(e=>e).Distinct().ToList(),
                    KeyValues = KeyValues,
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
