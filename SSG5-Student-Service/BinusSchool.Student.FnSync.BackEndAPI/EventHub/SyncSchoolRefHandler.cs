using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.StudentDb;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Student.FnSync.EventHub
{
    public class SyncSchoolRefHandler : FunctionsSyncRefTableHandlerFinal<StudentDbContext>
    {
#if DEBUG
        //private const string _hubName = "school-student-local";
#else
#endif
        private const string _hubName = "school-student";

        public SyncSchoolRefHandler(IServiceProvider services) : base(services, _hubName, "Student")
        {
        }

        [FunctionName(nameof(SyncSchoolRef))]
        public Task SyncSchoolRef(
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
