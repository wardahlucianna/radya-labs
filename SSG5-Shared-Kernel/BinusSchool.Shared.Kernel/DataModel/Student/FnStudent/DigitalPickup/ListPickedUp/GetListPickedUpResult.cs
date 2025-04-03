using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class GetListPickedUpResult
    {
        public string IdDigitalPickUp { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public string StudentName { get; set; }
        public DateTime QrScanTime { get; set; }
        public DateTime? PickupTime { get; set; }
    }
}
