using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
//using BinusSchool.User.FnCommunication.Message.Notification;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.User.FnCommunication.Message
{
    public class TestNotificationHandler : FunctionsHttpSingleHandler
    {
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            // NOTE: doing logic business here after savechange
            await Task.Delay(1);

            // send notification
            var recipients = new[] { "00000000-0000-0000-0000-000000000001", "TEACHER1" };
            
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS7")
                {
                    IdRecipients = recipients
                });
                collector.Add(message);
            }

            return Request.CreateApiResult2();
        }
    }
}
