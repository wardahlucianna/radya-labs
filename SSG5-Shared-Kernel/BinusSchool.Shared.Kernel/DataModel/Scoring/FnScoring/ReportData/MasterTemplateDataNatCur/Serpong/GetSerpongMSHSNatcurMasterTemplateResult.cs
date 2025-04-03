using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.CommonData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Serpong
{
    public class GetSerpongMSHSNatcurMasterTemplateResult
    {
        public CreateHeaderBNSReportRequest Header { get; set; }
        public string HtmlOutput { get; set; }
        public string StorageSetting { get; set; }
        public GetMasterTemplateResult_Margin Margin { get; set; }
        public GetMasterTemplateResult_GlobalSetting GlobalSetting { get; set; }
        public List<string> GenerateStatus { get; set; }
    }
}
