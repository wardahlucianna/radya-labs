using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.DigitalPickup
{
    public class GetStudentDigitalPickupHistoryRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
