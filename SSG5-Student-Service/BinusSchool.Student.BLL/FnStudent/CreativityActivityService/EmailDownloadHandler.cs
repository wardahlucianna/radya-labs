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
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class EmailDownloadHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public EmailDownloadHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request
               .ValidateBody<EmailDownloadResult, EmailDownloadValidator>();

            var EmailDownload = new EmailDownloadResult
            {
                IdUser = body.IdUser,
                Link = body.Link,
            };

            if (KeyValues.ContainsKey("GetEmailDownload"))
            {
                KeyValues.Remove("GetEmailDownload");
            }

            KeyValues.Add("GetEmailDownload", EmailDownload);
            var NotificationSupervisor = CAS2Notification(KeyValues,body.IdSchool);


            return Request.CreateApiResult2();
        }

        public static string CAS2Notification(IDictionary<string, object> KeyValues, string IdSchool)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetEmailDownload").Value;
            var GetEmailDownload = JsonConvert.DeserializeObject<EmailDownloadResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(IdSchool, "CAS2")
                {
                    IdRecipients = new List<string>
                    {
                        GetEmailDownload.IdUser,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
