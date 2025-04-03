using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting
{
    public class UpdateDigitalPickupSettingRequest
    {
        public List<string> IdGrade { get; set; }
        public string StartScanTime { get; set; }
        public string EndScanTime { get; set; }
    }
}
