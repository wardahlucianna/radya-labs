using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Audit.FnAuditTrail.TimeTrigger
{
    public static class FileMaintenance
    {
        /// <summary>
        /// This function run every 1am of UTC+7 format
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        [FunctionName("FileMaintenance")]
        public static async Task RunAsync([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Audit file maintenance executing");

            var list = new List<string>();
            
            log.LogInformation("Audit file maintenance executed");
        }
    }
}
