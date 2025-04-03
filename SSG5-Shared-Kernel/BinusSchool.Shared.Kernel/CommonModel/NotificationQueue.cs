using System.Collections.Generic;

namespace BinusSchool.Common.Model
{
    public class NotificationQueue
    {
        public NotificationQueue() {}

        public NotificationQueue(string idSchool, string idScenario)
        {
            IdSchool = idSchool;
            IdScenario = idScenario;
        }
        
        public string IdSchool { get; set; }
        public string IdScenario { get; set; }
        public IEnumerable<string> IdRecipients { get; set; }
        public IDictionary<string, object> KeyValues { get; set; }
    }
}
