using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class UpdatePickupStudentResult
    {
        public string IdDigitalPickUp { get; set; }
        public string IdSchool { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public string StudentName { get; set; }
        public DateTime QRScanTime { get; set; }
        public DateTime? PickupTime { get; set; }
    }
}
