using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting
{
    public class GetDigitalPickupSettingResult
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public TimeSpan? ScanStartTime { get; set; }
        public TimeSpan? ScanEndTime { get; set; }
        public bool CanDelete { get; set; }
    }
}
