using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode
{
    public class GetStudentForDigitalPickupRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdParent { get; set; }
        public bool IsHistory { get; set; }
    }
}
