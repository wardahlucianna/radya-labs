using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class SaveLockerBookingPeriodGradeRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<string> Grades { get; set; }
    }
}
