using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAvailableTimeRequest
    {
        public DateTime AppointmentDate { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string IdStudent { get; set; }
    }
}
