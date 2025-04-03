using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.CommonData;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class CreateHeaderFooterReportResult
    {
        public GetMasterTemplateResult_File Header { get; set; }
        public GetMasterTemplateResult_File Footer { get; set; }
    }
}
