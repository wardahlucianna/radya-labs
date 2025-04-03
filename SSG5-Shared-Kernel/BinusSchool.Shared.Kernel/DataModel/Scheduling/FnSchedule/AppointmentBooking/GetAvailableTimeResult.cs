using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAvailableTimeResult : CodeWithIdVm
    {
        public string IdVenue { get; set; }
        public string Venue { get; set; }
        public List<Time> Times { get; set; }
    }

    public class Time 
    {
        public string IdInvitationBookingSettingSchedule { get; set; }
        public string StartTimeInvitation { get; set; }
        public string EndTimeInvitation { get; set; }
        public bool IsDisabled { get; set; }
    }

}
