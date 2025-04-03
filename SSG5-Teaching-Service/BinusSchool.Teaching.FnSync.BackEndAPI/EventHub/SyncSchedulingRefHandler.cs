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
    public class SyncSchedulingRefHandler : FunctionsSyncRefTableHandlerFinal<TeachingDbContext>
    {
#if DEBUG
        private const string _hubName = "scheduling-teaching-local";
#else
        private const string _hubName = "scheduling-teaching";
#endif

        public SyncSchedulingRefHandler(IServiceProvider services) : base(services, _hubName, "Teaching")
        {
        }

        [FunctionName(nameof(SyncSchedulingRef))]
        public Task SyncSchedulingRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh5")]
            EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
