using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.UserDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.User.FnSync.EventHub
{
    public class SyncStudentRefHandler : FunctionsSyncRefTableHandlerFinal<UserDbContext>
    {
#if DEBUG
        private const string _hubName = "student-user-local";
#else
        private const string _hubName = "student-user";
#endif

        public SyncStudentRefHandler(IServiceProvider services) : base(services, _hubName, "User")
        {
        }

        [FunctionName(nameof(SyncStudentRef))]
        public Task SyncStudentRef(
            [EventHubTrigger(_hubName, Connection = "ConnectionStrings:SyncRefTable:EventHubs:eh4")]
            EventData eventMessage,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext,
            CancellationToken cancellationToken)
        {
            var message = Encoding.UTF8.GetString(eventMessage.Body);
            return Synchronize(message, executionContext.InvocationId, cancellationToken);
        }
    }
}
