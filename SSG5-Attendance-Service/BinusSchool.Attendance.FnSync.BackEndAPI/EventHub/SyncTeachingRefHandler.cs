using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.AttendanceDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Attendance.FnSync.EventHub
{
    public class SyncTeachingRefHandler : FunctionsSyncRefTableHandlerFinal<AttendanceDbContext>
    {
#if DEBUG
        private const string _hubName = "teaching-attendance-local";
#else
        private const string _hubName = "teaching-attendance";
#endif

        public SyncTeachingRefHandler(IServiceProvider services) : base(services, _hubName, "Attendance") {}

        [FunctionName(nameof(SyncTeachingRef))]
        public Task SyncTeachingRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh2")] EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
