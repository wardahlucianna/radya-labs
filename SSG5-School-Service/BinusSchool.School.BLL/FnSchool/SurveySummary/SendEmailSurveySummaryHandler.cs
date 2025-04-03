using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;

namespace BinusSchool.School.FnSchool.SurveySummary
{
    public class SendEmailSurveySummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public SendEmailSurveySummaryHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<SendEmailSurveySummaryRequest>();

            if (KeyValues.ContainsKey("EmailSurveySummary"))
            {
                KeyValues.Remove("EmailSurveySummary");
            }
            KeyValues.Add("EmailSurveySummary", param);

            if (param.IdScenario == "ESS1")
                ESS1Notification(KeyValues);
            else if (param.IdScenario == "ESS2")
                ESS2Notification(KeyValues);

            return Request.CreateApiResult2();
        }

        public static string ESS1Notification(IDictionary<string, object> KeyValues)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailSurveySummary").Value;
            var EmailDailyRecap = JsonConvert.DeserializeObject<SendEmailSurveySummaryRequest>(JsonConvert.SerializeObject(Object));
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(EmailDailyRecap.IdSchool, "ESS1")
                {
                    IdRecipients = new List<string> { EmailDailyRecap.IdUser },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string ESS2Notification(IDictionary<string, object> KeyValues)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailSurveySummary").Value;
            var EmailDailyRecap = JsonConvert.DeserializeObject<SendEmailSurveySummaryRequest>(JsonConvert.SerializeObject(Object));
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(EmailDailyRecap.IdSchool, "ESS2")
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
