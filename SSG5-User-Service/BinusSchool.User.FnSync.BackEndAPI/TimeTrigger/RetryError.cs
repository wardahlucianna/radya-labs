using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.UserDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.User.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<UserDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-user-local";
#else
        private const string _hubName = "retry-user";
#endif

        public RetryError(IServiceProvider services) : base(services, _hubName, "User")
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
