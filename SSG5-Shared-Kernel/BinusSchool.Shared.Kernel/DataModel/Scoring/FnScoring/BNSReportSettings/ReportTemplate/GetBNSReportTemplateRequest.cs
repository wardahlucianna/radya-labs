using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate
{
    public class GetBNSReportTemplateRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
