using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking
{
    public class AddStudentReservationRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; } 
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdlockerLocation { get; set; }
        public string IdlockerPosition { get; set; }
        public bool IsAgree { get; set; }
        public string Notes { get; set; }

    }
}
