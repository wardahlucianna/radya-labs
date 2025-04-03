using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.DigitalPickup
{
    public class GetStudentDigitalPickupHistoryResponse
    {
        public string StudentName { get; set; }
        public string Homeroom { get; set; }
        public string Date { get; set; }
        public string QRScannedTime { get; set; }
        public string PickedUpTime { get; set; }
    }
}
