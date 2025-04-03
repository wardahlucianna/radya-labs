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
    public class SyncTeachingRefHandler : FunctionsSyncRefTableHandlerFinal<StudentDbContext>
    {
#if DEBUG
        //private const string _hubName = "teaching-student-local";
#else
        
#endif
        private const string _hubName = "teaching-student";

        public SyncTeachingRefHandler(IServiceProvider services) : base(services, _hubName, "Student")
        {
        }

        [FunctionName(nameof(SyncTeachingRef))]
        public Task SyncTeachingRef(
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
