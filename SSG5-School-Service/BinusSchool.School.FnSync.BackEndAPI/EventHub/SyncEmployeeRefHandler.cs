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
    public class SyncEmployeeRefHandler : FunctionsSyncRefTableHandler2<SchoolDbContext>
    {
#if DEBUG
        private const string _hubName = "employee-school-local";
#else
        private const string _hubName = "employee-school";
#endif

        public SyncEmployeeRefHandler(IServiceProvider services) : base(services, _hubName)
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
