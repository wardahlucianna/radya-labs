using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.StudentDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Student.FnSync.EventHub
{
    public class SyncSchedulingRefHandler : FunctionsSyncRefTableHandlerFinal<StudentDbContext>
    {
#if DEBUG
        //private const string _hubName = "scheduling-student-local";
#else
#endif
        private const string _hubName = "scheduling-student";

        public SyncSchedulingRefHandler(IServiceProvider services) : base(services, _hubName, "Student")
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
