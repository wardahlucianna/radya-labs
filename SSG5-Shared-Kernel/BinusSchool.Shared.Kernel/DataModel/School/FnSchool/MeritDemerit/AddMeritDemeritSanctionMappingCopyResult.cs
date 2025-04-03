using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritSanctionMappingCopyResult
    {
        public string CountSucces { get; set; }
        public List<DemeritSanctionMapping> DemeritSanctionMappingFailed { get; set; }
    }

    public class DemeritSanctionMapping
    {
        public string AcademicYear { get; set; }
        public string SunctionName { get; set; }
        public string DemeritMin { get; set; }
        public string DemeritMax { get; set; }
        public string AttentionBy { get; set; }
    }
}
