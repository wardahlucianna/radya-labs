using System;

namespace BinusSchool.Common.Model
{
    public class NotificationTemplate : CodeWithIdVm
    {
        public string IdFeatureSchool { get; set; }
        [Obsolete("No longer used")]

        public string FeatureCode { get; set; }
        public string Scenario { get; set; }
        public string Title { get; set; }
        public string Push { get; set; }
        public string Email { get; set; }
        public bool EmailIsHtml { get; set; }
    }
}
