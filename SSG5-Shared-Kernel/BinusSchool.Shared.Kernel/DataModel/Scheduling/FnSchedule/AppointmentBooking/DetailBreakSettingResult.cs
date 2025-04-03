using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class DetailBreakSettingResult : CodeWithIdVm
    {
        public string IdInvitationBookingSettingBreak { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string BreakName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public BreakType BreakType { get; set; }
        public ItemValueVm Grade { get; set; }
    }
}
