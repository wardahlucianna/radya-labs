using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListInvitationBookingSettingRequest : CollectionSchoolRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string InvitationStartDate { get; set; }
        public string InvitationEndDate { get; set; }
        public StatusInvitationBookingSetting Status { get; set; }
        public bool IsMyInvitationBooking { get; set; }
    }
}
