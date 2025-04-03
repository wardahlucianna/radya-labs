using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreSummarySubjectByHomeroomResult
    {
        public GetStudentScoreSummarySubjectByHomeroomResult()
        {
            StudentScoreSummaryList = new List<StudentScoreSummarySubjectVm>();
        }
        public List<StudentScoreSummarySubjectVm> StudentScoreSummaryList { set; get; }
    }

    public class StudentScoreSummarySubjectVm
    {
        public string Class { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }
}
