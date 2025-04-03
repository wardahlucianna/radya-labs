using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class GetSettingScoreSummaryTabDetailResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public List<CodeWithIdVm> Grades { get; set; }
        public List<GetSettingScoreSummaryTabResult_RoleAccessed> RoleAccessed { get; set; }
        public ItemValueVm? ReportType { get; set; }
        public int OrderNumberScoreSummaryTab { get; set; }
        public bool IsExportExcel { get; set; }
    }
}
