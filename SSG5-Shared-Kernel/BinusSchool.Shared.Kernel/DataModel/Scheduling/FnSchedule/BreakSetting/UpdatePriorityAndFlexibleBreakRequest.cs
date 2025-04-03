using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting
{
    public class UpdatePriorityAndFlexibleBreakRequest
    {
        public bool IsPrority { get; set; }
        public bool IsChecked { get; set; }
        public string IdInvitationBookingSettingSchedule { get; set; }
        public string BreakName { get; set; }
        public string IdInvitationBookingSettingBreak { get; set; }
    }
}
