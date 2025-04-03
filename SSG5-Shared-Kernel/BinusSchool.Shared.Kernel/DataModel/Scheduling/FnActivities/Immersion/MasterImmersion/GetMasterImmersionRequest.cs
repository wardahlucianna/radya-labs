using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetMasterImmersionRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdImmersionPeriod { get; set; }
    }
}
