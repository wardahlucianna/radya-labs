using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.CommonData;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Simprug
{
    public class GetSimprugPYPMasterTemplateResult
    {
        //public CreateHeaderBNSReportRequest Header { get; set; }
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
        public string StorageSetting { get; set; }
        public TemplateBNSSettingVm TemplateBNSSetting { set; get; }
        public GetMasterTemplateResult_File Header { get; set; }
        public GetMasterTemplateResult_File Footer { get; set; }
    }
}
