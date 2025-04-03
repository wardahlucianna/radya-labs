using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetScoreSummaryStudentScoreBySubjectTypeResult
    {
        public ItemValueVm Subject { set; get; }
        public NameValueVm Teacher { set; get; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }

    public class GetScoreSummaryStudentScoreBySubjectTypeResult_CounterDetail
    {
        public string IdStudent { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
    }

    public class GetScoreSummaryStudentScoreBySubjectTypeResult_Counter
    {
        public string IdStudent { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public List<string> Counter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
    }
}
