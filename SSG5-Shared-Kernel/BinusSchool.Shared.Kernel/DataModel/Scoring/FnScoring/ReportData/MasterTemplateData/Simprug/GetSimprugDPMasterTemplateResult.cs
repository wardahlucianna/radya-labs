using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Simprug
{
    public class GetSimprugDPMasterTemplateResult
    {
        public string ContainerName { get; set; }
        public CreateHeaderBNSReportRequest Header { get; set; }
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
        public string StorageSetting { get; set; }
        public TemplateBNSSettingVm TemplateBNSSetting { set; get; }
    }
}
