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
    public class SyncAttendanceRefHandler : FunctionsSyncRefTableHandlerFinal<SchedulingDbContext>
    {
#if DEBUG
        private const string _hubName = "attendance-scheduling-local";
#else
        private const string _hubName = "attendance-scheduling";
#endif

        public SyncAttendanceRefHandler(IServiceProvider services) : base(services, _hubName, "Scheduling")
        {
        }

        [FunctionName(nameof(SyncAttendanceRef))]
        public Task SyncAttendanceRef(
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
