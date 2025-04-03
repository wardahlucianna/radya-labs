using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListRecapRequest : CollectionSchoolRequest
    {
        public string IdInvitationBookingSetting { get; set; }
        public string IdUserTeacher { get; set; }
        public InvitationBookingStatus? Status { get; set; }
    }
}
