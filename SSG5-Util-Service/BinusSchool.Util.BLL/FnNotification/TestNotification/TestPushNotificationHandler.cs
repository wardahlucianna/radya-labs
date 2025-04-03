using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using FirebaseAdmin.Messaging;

namespace BinusSchool.Util.FnNotification.TestNotification
{
    public class TestPushNotificationHandler : FunctionsHttpSingleHandler
    {
        private readonly INotificationManager _notificationManager;

        public TestPushNotificationHandler(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<MulticastMessage>();
            var queueMessageId = await _notificationManager.SendPushNotification(body);
            
            return Request.CreateApiResult2($"Queue Message Id: {queueMessageId}" as object);
        }
    }
}
