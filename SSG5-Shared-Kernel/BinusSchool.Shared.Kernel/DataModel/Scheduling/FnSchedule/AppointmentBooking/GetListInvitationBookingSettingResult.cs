using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListInvitationBookingSettingResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string InvitationName { get; set; }
        public DateTime InvitationStartDate { get; set; }
        public DateTime InvitationEndDate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanEditVenueOnly { get; set; }
    }
}
