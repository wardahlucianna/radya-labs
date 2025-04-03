using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class GetAllSubjectForScoreSummaryTabSectionSubjectRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdScoreSummaryTab { get; set; }
        public string? IdScoreSummaryTabSection { get; set; }
    }
}
