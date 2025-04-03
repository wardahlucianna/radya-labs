using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.Blocking
{
    public class GetBlockingDashboardResult
    {
        public bool IsBlockingWebsite { get; set; }

        public string BlockingMessageWebsite { get; set; }

        public bool IsBlockingFeature { get; set; }

        public List<string> BlockingMessageFeature { get; set; }

        public bool ShowAttendanceSummary { get; set; }

        public bool ShowWorkhabbit { get; set; }

        public bool ShowSchedule { get; set; }

        public bool ShowInformation { get; set; }

        public bool ShowDisciplineSystem { get; set; }

    }
}
