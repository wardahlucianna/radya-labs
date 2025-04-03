using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class ExportExcelMasterImmersionRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdImmersionPeriod { get; set; }
    }
}
