using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.AttendanceDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Attendance.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<AttendanceDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-attendance-local";
#else
        private const string _hubName = "retry-attendance";
#endif

        public RetryError(IServiceProvider services) : base(services, _hubName, "Attendance")
        {
        }

        [FunctionName(nameof(RetryError))]
        public Task RunAsync(
            [TimerTrigger("0 */15 * * * *"
#if DEBUG
                , RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
            => RetrySyncAsync(cancellationToken);
    }
}
