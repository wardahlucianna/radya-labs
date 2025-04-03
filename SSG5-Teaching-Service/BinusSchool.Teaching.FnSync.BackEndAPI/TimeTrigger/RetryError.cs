using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.TeachingDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Teaching.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<TeachingDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-teaching-local";
#else
        private const string _hubName = "retry-teaching";
#endif

        public RetryError(IServiceProvider services) : base(services, _hubName, "Teaching")
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
