using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.TeachingDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Teaching.FnSync.EventHub
{
    public class SyncEmployeeRefHandler : FunctionsSyncRefTableHandlerFinal<TeachingDbContext>
    {
#if DEBUG
        private const string _hubName = "employee-teaching-local";
#else
        private const string _hubName = "employee-teaching";
#endif

        public SyncEmployeeRefHandler(IServiceProvider services) : base(services, _hubName, "Teaching")
        {
        }

        [FunctionName(nameof(SyncEmployeeRef))]
        public Task SyncEmployeeRef(
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
