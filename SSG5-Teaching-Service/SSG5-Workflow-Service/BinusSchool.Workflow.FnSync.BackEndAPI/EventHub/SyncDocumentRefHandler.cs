using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.WorkflowDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Workflow.FnSync.EventHub
{
    public class SyncDocumentRefHandler : FunctionsSyncRefTableHandlerFinal<WorkflowDbContext>
    {
#if DEBUG
        private const string _hubName = "document-workflow-local";
#else
        private const string _hubName = "document-workflow";
#endif

        public SyncDocumentRefHandler(IServiceProvider services) : base(services, _hubName, "Workflow")
        {
        }

        [FunctionName(nameof(SyncDocumentRef))]
        public Task SyncDocumentRef(
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
