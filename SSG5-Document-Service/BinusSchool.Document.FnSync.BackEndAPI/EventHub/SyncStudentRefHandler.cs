using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.DocumentDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Document.FnSync.EventHub
{
    public class SyncStudentRefHandler : FunctionsSyncRefTableHandlerFinal<DocumentDbContext>
    {
#if DEBUG
        //private const string _hubName = "student-document-local";
#else
#endif
        private const string _hubName = "student-document";

        public SyncStudentRefHandler(IServiceProvider services) : base(services, _hubName, "Document")
        {
        }

        [FunctionName(nameof(SyncStudentRef))]
        public Task SyncStudentRef(
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
