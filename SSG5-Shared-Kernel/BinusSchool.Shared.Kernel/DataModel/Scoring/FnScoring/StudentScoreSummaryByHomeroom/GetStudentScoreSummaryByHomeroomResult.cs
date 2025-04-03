using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreSummaryByHomeroomResult
    {
        public string Class { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }

    public class StudentCounterScoreVm
    {
        public string IdSchool { set; get; }
        public string Class { set; get; }
        public string IdStudent { set; get; }        
        public string StudentName { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string SubjectLevelName { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string IdLesson { set; get; }
        public string Counter { set; get; }
        public decimal Score { set; get; }
        public decimal? CounterScore { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public string IdHomeroomStudent { set; get; }
        public string CategoryAdditionalScore { set; get; }

    }

    public class ScoreAdditionalCodeVm
    {
        public decimal Score { set; get; }
        public string IdSchool { set; get; }
        public string LongDesc { set; get; }
        public bool ShowScoreAsNA { set; get; }   
        public string CategoryScore { set; get; }

    }
}
