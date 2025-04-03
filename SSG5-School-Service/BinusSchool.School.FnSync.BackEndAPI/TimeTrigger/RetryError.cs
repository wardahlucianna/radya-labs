﻿using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.SchoolDb;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.School.FnSync.TimeTrigger
{
    public class RetryError : FunctionsSyncRefTableHandlerFinal<SchoolDbContext>
    {
#if DEBUG
        private const string _hubName = "retry-school-local";
#else
        private const string _hubName = "retry-school";
#endif

        public RetryError(IServiceProvider services) : base(services, _hubName, "School")
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
