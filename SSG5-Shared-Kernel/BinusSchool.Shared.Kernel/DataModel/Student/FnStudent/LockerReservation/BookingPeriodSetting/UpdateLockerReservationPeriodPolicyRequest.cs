using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class UpdateLockerReservationPeriodPolicyRequest
    {
        public string IdLockerReservationPeriod { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PolicyMessage { get; set; }
    }
}
