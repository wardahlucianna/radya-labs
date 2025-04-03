using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class AddUpdateSettingScoreSummaryTabRequest
    {
        public string IdScoreSummaryTab { get; set; }
        public string Description { get; set; }
        public string? IdReportType { get; set; }
        public bool IsExportExcel { get; set; }
        public int OrderNumber { get; set; }
        public string IdSchool { get; set; }
        public List<string> Grades { get; set; }
        public List<string> Roles { get; set; }
    }
}
