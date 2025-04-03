using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Employee.FnStaff.Teacher
{
    public class UpdateExpatriateFormalitiesRequest
    {
        public string IdBinusian { get; set; }
        public string KITASNumber { get; set; }
        public string KITASSponsor { get; set; }
        public string IdKITASStatus { get; set; }
        //public string IdKITASStatusDesc { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string IMTANumber { get; set; }
        public string IdIMTASchoolLevel { get; set; }
        //public string IdIMTASchoolLevelDesc { get; set; }
        public string IdIMTAMajorAssignPosition { get; set; }
        //public string IdIMTAMajorAssignPositionDesc { get; set; }
        public DateTime? IMTAExpDate { get; set; }
    }
}
