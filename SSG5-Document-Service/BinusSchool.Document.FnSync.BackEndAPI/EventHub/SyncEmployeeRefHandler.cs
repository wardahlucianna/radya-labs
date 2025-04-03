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
    public class SyncEmployeeRefHandler : FunctionsSyncRefTableHandlerFinal<DocumentDbContext>
    {
#if DEBUG
        //private const string _hubName = "employee-document-local";
#else
#endif
        private const string _hubName = "employee-document";

        public SyncEmployeeRefHandler(IServiceProvider services) : base(services, _hubName, "Document")
        {
        }

        [FunctionName(nameof(SyncEmployeeRef))]
        public Task SyncEmployeeRef(
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
