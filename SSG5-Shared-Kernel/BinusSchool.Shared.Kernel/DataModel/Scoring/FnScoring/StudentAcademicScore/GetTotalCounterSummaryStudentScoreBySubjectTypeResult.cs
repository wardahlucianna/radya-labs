using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetTotalCounterSummaryStudentScoreBySubjectTypeResult
    {
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }
}
