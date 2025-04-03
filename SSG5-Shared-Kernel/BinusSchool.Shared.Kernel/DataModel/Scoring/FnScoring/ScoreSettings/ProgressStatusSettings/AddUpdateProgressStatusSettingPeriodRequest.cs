using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class AddUpdateProgressStatusSettingPeriodRequest
    {
        public string IdAcademicYear { set; get; }
        public List<string> Grades { set; get; }
        public string? IdApprovalWorkflow { set; get; }
        public bool NeedApproval { set; get; }
        public DateTime PeriodStartDate { set; get; }
        public DateTime PeriodEndDate { set; get; }
    }
}
