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
    public class SyncTeachingRefHandler : FunctionsSyncRefTableHandlerFinal<SchoolDbContext>
    {
#if DEBUG
        private const string _hubName = "teaching-school-local";
#else
        private const string _hubName = "teaching-school";
#endif

        public SyncTeachingRefHandler(IServiceProvider services) : base(services, _hubName, "School")
        {
        }

        [FunctionName(nameof(SyncTeachingRef))]
        public Task SyncTeachingRef(
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
