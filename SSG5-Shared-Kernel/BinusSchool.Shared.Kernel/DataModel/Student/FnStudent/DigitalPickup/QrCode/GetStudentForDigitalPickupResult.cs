using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode
{
    public class GetStudentForDigitalPickupResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public ItemValueVm IdGrade { get; set; }
        public bool Selected { get; set; }
    }
}
