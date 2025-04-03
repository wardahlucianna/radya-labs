using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.DocumentDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Document.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<DocumentDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-document-local";
#else
        private const string _hubName = "retry-document";
#endif

        public RetryError(IServiceProvider services) : base(services, _hubName, "Document")
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
