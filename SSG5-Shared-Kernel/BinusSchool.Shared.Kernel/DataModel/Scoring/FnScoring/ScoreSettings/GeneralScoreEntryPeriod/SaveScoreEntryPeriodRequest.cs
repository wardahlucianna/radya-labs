using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.GeneralScoreEntryPeriod
{
    public class SaveScoreEntryPeriodRequest
    {
        public string IdAcademicYear { get; set; }
        public string TermCode { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public DateTime? UpdatePeriodStartDate { get; set; }
        public DateTime? UpdatePeriodEndDate { get; set; }
        public List<SaveScoreEntryPeriodRequest_Subjects> SubjectList { get; set; }
        public string Reason { get; set; }
    }

    public class SaveScoreEntryPeriodRequest_Subjects
    {
        public string IdSubjectScoreSetting { get; set; }
        public string IdSubject { get; set; }
    }
}
