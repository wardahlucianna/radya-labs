using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTemplate
{
    public class GetReportTemplateResult : CodeWithIdVm
    {
        public string IdReportTemplate { set; get; }
        public string Description { set; get; }
        public string LongDesc { set; get; }
        public string Preview { set; get; }
        public string Status { set; get; }
    }
}
