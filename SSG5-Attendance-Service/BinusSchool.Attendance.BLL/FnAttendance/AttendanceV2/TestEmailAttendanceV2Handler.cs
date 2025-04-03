using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class TestEmailAttendanceV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public TestEmailAttendanceV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            #region Notification
            var Notification = CAS1Notification(KeyValues, AuthInfo);

            #endregion

            return Request.CreateApiResult2();
        }

        public static string CAS1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailRequestExperienceResult>(JsonConvert.SerializeObject(Object));
            List<string> coba = new List<string>();
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD8")
                {
                    IdRecipients = coba,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
