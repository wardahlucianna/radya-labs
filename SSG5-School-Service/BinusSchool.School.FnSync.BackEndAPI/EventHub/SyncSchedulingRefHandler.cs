using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.SchoolDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.School.FnSync.EventHub
{
    public class SyncSchedulingRefHandler : FunctionsSyncRefTableHandlerFinal<SchoolDbContext>
    {
#if DEBUG
        private const string _hubName = "scheduling-school-local";
#else
        private const string _hubName = "scheduling-school";
#endif

        public SyncSchedulingRefHandler(IServiceProvider services) : base(services, _hubName, "School")
        {
        }

        [FunctionName(nameof(SyncSchedulingRef))]
        public Task SyncSchedulingRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh3")]
            EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
