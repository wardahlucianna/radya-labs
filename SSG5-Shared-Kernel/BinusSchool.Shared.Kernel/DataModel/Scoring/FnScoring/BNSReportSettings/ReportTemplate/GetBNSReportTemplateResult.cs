using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate
{
    public class GetBNSReportTemplateResult : CodeWithIdVm
    {
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public string Template { get; set; }
        public string StyleCss { get; set; }
        public int? ApprovalStatus { get; set; }
        public bool CurrentStatus { get; set; }
        public bool ShowDeleteButton { get; set; }
        public bool ShowEditButton { get; set; }
    }
}
