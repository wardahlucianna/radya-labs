using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByLevel
{
    public class GetScoreSummaryByLevelResult
    {
        public CodeWithIdVm Level { set; get; }
        public int OrderNumber { get; set; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }

    public class GetScoreSummaryByLevelResult_CounterDetail
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

    public class GetScoreSummaryByLevelResult_Counter
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public List<string> Counter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

    public class GetScoreSummaryByLevelResult_CounterStudentScore
    {
        public string IdStudent { set; get; }
        public string IdCounter { set; get; }
        public decimal? Score { set; get; }
        public string? Category { set; get; }
    }

    public class GetScoreSummaryByLevelResult_SubjectCounter
    {
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdCounter { set; get; }
    }
    public class GetScoreSummaryByLevelResult_trscore
    {
        public string IdStudent { set; get; }
        public string IdCounter { set; get; }
        public decimal Score { set; get; }
    }
}
