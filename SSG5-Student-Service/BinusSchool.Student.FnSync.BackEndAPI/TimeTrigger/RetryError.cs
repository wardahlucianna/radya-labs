using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.StudentDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Student.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<StudentDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-student-local";
#else
        private const string _hubName = "retry-student";
#endif
        
        public RetryError(IServiceProvider services) : base(services, _hubName, "Student")
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
