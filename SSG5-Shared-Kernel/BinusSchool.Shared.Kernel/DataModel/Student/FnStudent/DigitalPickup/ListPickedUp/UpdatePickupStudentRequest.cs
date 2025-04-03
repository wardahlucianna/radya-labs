using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class UpdatePickupStudentRequest
    {
        public int Status { get; set; }
        public List<string> IdDigitalPickup { get; set; }

    }
}
