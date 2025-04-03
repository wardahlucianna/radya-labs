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
    public class SyncStudentRefHandler : FunctionsSyncRefTableHandlerFinal<SchedulingDbContext>
    {
#if DEBUG
        private const string _hubName = "student-scheduling-local";
#else
        private const string _hubName = "student-scheduling";
#endif

        public SyncStudentRefHandler(IServiceProvider services) : base(services, _hubName, "Scheduling")
        {
        }

        [FunctionName(nameof(SyncStudentRef))]
        public Task SyncStudentRef(
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
