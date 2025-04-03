using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.UserDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.User.FnSync.EventHub
{
    public class SyncSchedulingRefHandler : FunctionsSyncRefTableHandlerFinal<UserDbContext>
    {
#if DEBUG
        private const string _hubName = "scheduling-user-local";
#else
        private const string _hubName = "scheduling-user";
#endif

        public SyncSchedulingRefHandler(IServiceProvider services) : base(services, _hubName, "User")
        {
        }

        [FunctionName(nameof(SyncSchedulingRef))]
        public Task SyncSchedulingRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh4")]
            EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
