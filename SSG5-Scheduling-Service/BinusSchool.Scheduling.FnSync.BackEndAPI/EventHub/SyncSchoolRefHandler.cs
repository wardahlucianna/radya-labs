using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.SchedulingDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Scheduling.FnSync.EventHub
{
    public class SyncSchoolRefHandler : FunctionsSyncRefTableHandlerFinal<SchedulingDbContext>
    {
#if DEBUG
        private const string _hubName = "school-scheduling-local";
#else
        private const string _hubName = "school-scheduling";
#endif

        public SyncSchoolRefHandler(IServiceProvider services) : base(services, _hubName, "Scheduling")
        {
        }

        [FunctionName(nameof(SyncSchoolRef))]
        public Task SyncSchoolRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh1")]
            EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
