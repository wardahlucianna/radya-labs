using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetSubjectScoreFinalCombinedResult
    {
        public ItemValueVm Subject { set; get; }
        public ItemValueVm SubjectType { set; get; }
        public ItemValueVm SubjectLevel { set; get; }
        public List<NameValueVm> Teacher { set; get; }
        public List<GetSubjectScoreFinalCombinedResult_SmtScore> SmtScore { set; get; }
        public GetSubjectScoreFinalCombinedResult_Final FinalScore { set; get; }
        public GetSubjectScoreFinalCombinedResult_Final FinalGrade { set; get; }
        public decimal? MaxScore { set; get; }
        public bool? ShowGradingAsScore { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_SmtScore
    {
        public string IdSmt { set; get; }
        public string SmtDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetSubjectScoreFinalCombinedResult_TermScore> TermScore { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_TermScore
    {
        public string IdTerm { set; get; }
        public string TermDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetSubjectScoreFinalCombinedResult_ComponentScore> ComponentScore { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_ComponentScore
    {
        public int? OrderNo { set; get; }
        public string IdComponent { set; get; }
        public string ComponentDesc { set; get; }
        public string ComponentShortDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetSubjectScoreFinalCombinedResult_SubComponentScore> SubComponentScore { set; get; }
        public bool ShowGradingAsScore { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_SubComponentScore
    {
        public int? OrderNo { set; get; }
        public string IdSubComponent { set; get; }
        public string SubComponentDesc { set; get; }
        public string SubComponentShortDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetSubjectScoreFinalCombinedResult_SubComponentCounterScore> SubComponentCounterScore { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_SubComponentCounterScore
    {
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterDesc { set; get; }
        public string SubComponentCounterShortDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_Final
    {
        public string FinalDesc { set; get; }
        public string Score { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_CounterDetail
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_Counter
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public List<GetSubjectScoreFinalCombinedResult_SubjectCounter> Counter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_trscore
    {
        public string IdStudent { set; get; }
        public string IdCounter { set; get; }
        public decimal Score { set; get; }
    }

    public class GetSubjectScoreFinalCombinedResult_SubjectCounter
    {
        public string IdSubject { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdCounter { set; get; }
    }
}
