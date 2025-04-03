using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.SchedulingDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Scheduling.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<SchedulingDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-scheduling-local";
#else
        private const string _hubName = "retry-scheduling";
#endif

        public RetryError(IServiceProvider services) : base(services, _hubName, "Scheduling")
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
