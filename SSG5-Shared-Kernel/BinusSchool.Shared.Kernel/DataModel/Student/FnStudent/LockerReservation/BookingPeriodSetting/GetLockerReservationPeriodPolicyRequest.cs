using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerReservationPeriodPolicyRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
