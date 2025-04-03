using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model
{
    public class NotificationHistory
    {
        public string IdSchool { get; set; }
        public string IdFeature { get; set; }
        [Obsolete("No longer used")]
        public string FeatureCode { get; set; }
        public string Title { get; set; }
        public string Scenario { get; set; }
        public string Content { get; set; }
        public string Action { get; set; }
        public string Data { get; set; }
        public bool IsBlast { get; set; }
        public NotificationType NotificationType { get; set; }
        public IEnumerable<string> IdUserRecipients { get; set; }
    }
}
