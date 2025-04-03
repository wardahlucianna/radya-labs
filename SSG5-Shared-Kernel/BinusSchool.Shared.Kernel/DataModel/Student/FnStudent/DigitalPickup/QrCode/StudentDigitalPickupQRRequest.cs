using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode
{
    public class StudentDigitalPickupQRRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
    }
}
