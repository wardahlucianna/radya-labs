using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class GetProgressStudentStatusMappingResult
    {
        public string IdProgressStudentStatusMapping { get; set; }
        public string ProgressStatusDesc { get; set; }
        public string ProgressStatusBahasaDesc { get; set; }
        public string ProgressOrNational { get; set; }
        public bool EnableAgreement { get; set; }
        public bool EnableHideReportCard { get; set; }
        public int IdStudentStatus { get; set; }
    }
}
