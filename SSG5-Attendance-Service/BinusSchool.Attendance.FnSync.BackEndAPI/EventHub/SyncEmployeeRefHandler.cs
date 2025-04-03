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
    public class SyncEmployeeRefHandler : FunctionsSyncRefTableHandlerFinal<AttendanceDbContext>
    {
#if DEBUG
        private const string _hubName = "employee-attendance-local";
#else
        private const string _hubName = "employee-attendance";
#endif

        public SyncEmployeeRefHandler(IServiceProvider services) : base(services, _hubName, "Attendance")
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
