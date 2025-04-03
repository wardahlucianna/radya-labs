using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Employee.FnStaff.Teacher
{
    public class SetExpatriateFormalities
    {
        public string KITASNumber { get; set; }
        public string KITASSponsor { get; set; }
        public ItemValueVm IdKITASStatus { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string IMTANumber { get; set; }
        public ItemValueVm IdIMTASchoolLevel { get; set; }
        public ItemValueVm IdIMTAMajorAssignPosition { get; set; }
        public DateTime? IMTAExpDate { get; set; }
    }
}
