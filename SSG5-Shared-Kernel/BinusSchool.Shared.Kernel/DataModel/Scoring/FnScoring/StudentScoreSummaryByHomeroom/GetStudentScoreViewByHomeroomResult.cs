using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreViewByHomeroomResult
    {
        public ItemValueVm Homeroom { set; get; }
        public string Class { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public List<SubjectTypeVm> SubjectTypeList { set; get; }
    }

    public class SubjectTypeVm
    {
        public string IdSubjectType { set; get; }
        public string SubjectTypeName { set; get; }
        public List<SubjectVm> SubjectList { set; get; }
        public bool IsOverallScore { set; get; }
        public bool IsOverallScoreAvailable { set; get; }
        public decimal? OverallScore { set; get; }
        public decimal? OverallScoreSem2 { set; get; }
        public int Semester { get; set; }
    }

    public class SubjectVm
    {
        public string IdSubjectScoreSetting { set; get; }
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectLevel { set; get; }
        public decimal Score { set; get; }
        public decimal ScoreSmt2 { get; set; } //semester2
        public string OverallSemesterScore { get; set; }
        public string ScoreColor { set; get; }
        public string ScoreText { set; get; }
        public string ScoreTextSmt2 { get; set; } //semester2
        public string ScoreView { set; get; }
        public string ScoreViewSmt2 { get; set; } //semester2
        public bool IsMappingSubject { get; set; }
        public bool ShowGradingAsScore { get; set; }
    }

    public class StudentSubjectScoreVm
    {
        public string IdSubjectScoreSetting { set; get; }
        public string IdStudent { set; get; }
        public string IdSubjectType { set; get; }
        public string SubjectTypeName { set; get; }
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectLevel { set; get; }
        public string SubjectLevelName { get; set; }
        public decimal Score { set; get; }
        
        public string ScoreText { set; get; }
        public string ScoreView { set; get; }
        public bool IsMappingSubject { get; set; }

        public bool ShowGradingAsScore { set; get; }
    }

    public class GetStudentScoreViewByHomeroomResult_SubjectScoreSemester
    {
        public string IdStudent { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectType { set; get; }
        public string SubjectTypeName { set; get; }
        public bool ShowGradingAsScore { set; get; }
        public decimal MinScore { set; get; }
        public decimal Score { set; get; }
        public string ScoreText { set; get; }
        public string ScoreView { set; get; }
        public string SubjectLevelName { set; get; }
        public string FinalScore { get; set; }
    }
}
