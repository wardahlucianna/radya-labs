using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetSchedulePreviewResult : CodeWithIdVm
    {
        public string IdInvitationBookingSetting { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int QuotaSlot { get; set; }
        public int Duration { get; set; }
        public bool IsAvailable { get; set; }
        public string BreakName { get; set; }
        public string AvailableDescription { get; set; }
    }
}
