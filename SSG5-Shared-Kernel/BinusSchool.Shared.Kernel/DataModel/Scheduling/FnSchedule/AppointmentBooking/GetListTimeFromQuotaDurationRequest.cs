using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListTimeFromQuotaDurationRequest
    {
        public string IdInvitationBookingSetting { get; set; }
        public DateTime DateInvitation { get; set; }
        public string IdGrade { get; set; }
    }
}
