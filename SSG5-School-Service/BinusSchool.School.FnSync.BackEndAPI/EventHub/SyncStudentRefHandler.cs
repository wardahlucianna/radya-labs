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
    public class SyncStudentRefHandler : FunctionsSyncRefTableHandlerFinal<SchoolDbContext>
    {
#if DEBUG
        private const string _hubName = "student-school-local";
#else
        private const string _hubName = "student-school";
#endif

        public SyncStudentRefHandler(IServiceProvider services) : base(services, _hubName, "School")
        {
        }

        [FunctionName(nameof(SyncStudentRef))]
        public Task SyncStudentRef(
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
