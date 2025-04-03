using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.TeachingDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Teaching.FnSync.EventHub
{
    public class SyncUserRefHandler : FunctionsSyncRefTableHandlerFinal<TeachingDbContext>
    {
#if DEBUG
        private const string _hubName = "user-teaching-local";
#else
        private const string _hubName = "user-teaching";
#endif

        public SyncUserRefHandler(IServiceProvider services) : base(services, _hubName, "Teaching")
        {
        }

        [FunctionName(nameof(SyncUserRef))]
        public Task SyncUserRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh2")]
            EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
