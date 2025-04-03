using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAllChildDateilByParentRequest
    {
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
        public string IdInvitationBookingSetting { get; set; }

        //role : Parent/Staff
        public string Role { get; set; }
        public bool IsSiblingSameTime { get; set; }
    }
}
