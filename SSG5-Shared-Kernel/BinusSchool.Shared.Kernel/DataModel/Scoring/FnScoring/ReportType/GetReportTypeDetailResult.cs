using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportType
{
    public class GetReportTypeDetailResult : ItemValueVm
    {
        public string IdSchool { get; set; }
        public bool CurrentStatus { set; get; }
    }
}
