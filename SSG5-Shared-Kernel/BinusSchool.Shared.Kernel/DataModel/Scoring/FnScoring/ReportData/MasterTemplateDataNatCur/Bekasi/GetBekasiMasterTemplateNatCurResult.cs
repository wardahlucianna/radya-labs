using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.CommonData;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Bekasi
{
    public class GetBekasiMasterTemplateNatCurResult
    {
        public string HtmlOutput { get; set; }
        public string StorageSetting { get; set; }
        public GetMasterTemplateResult_Margin Margin { get; set; }
        public GetMasterTemplateResult_GlobalSetting GlobalSetting { get; set; }
        public List<string> GenerateStatus { get; set; }
        public GetMasterTemplateResult_File Header { get; set; }
        public GetMasterTemplateResult_File Footer { get; set; }
    }
}
