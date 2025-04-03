using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTemplate
{
    public class GetReportTemplateRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public bool? Status { set; get; }
    }
}
