using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData
{
    public class CreateSignatureBNSReportRequest
    {
        public string TemplateSignature { get; set; }
        public string TemplateCode { get; set; }
        public NameValueVm CA { get; set; }
        public NameValueVm CA2 { get; set; }
        public NameValueVm Principal { get; set; }
    }
}
