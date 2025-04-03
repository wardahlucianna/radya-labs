using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    #region comp_ib_2023_subject_score
    public class GetComponentIB2023SubjectScoreResult
    {
        public List<GetComponentIB2023SubjectScoreResult_Header> Header { get; set; }
        public List<GetComponentIB2023SubjectScoreResult_Body> BodySubjectScore { get; set; }
        public GetComponentIB2023SubjectScoreResult_BodyDataFinalCombine BodySubjectFinalCombine { get; set; }
        public GetComponentIB2023SubjectScoreResult_Footer Footer { get; set; }
    }

    public class GetComponentIB2023SubjectScoreResult_Header : NameValueVm
    {
        public List<string> ComponentList { get; set; }
    }
    public class GetComponentIB2023SubjectScoreResult_Body
    {
        public NameValueVm Subject { get; set; }
        public string Teacher { get; set; }
        public List<GetComponentIB2023SubjectScoreResult_ComponentScore> ComponentList { get; set; }
        public string TotalScore { get; set; }
        public string GradeScore { get; set; }
    }

    public class GetComponentIB2023SubjectScoreResult_BodyDataFinalCombine
    {
        public List<GetComponentIB2023SubjectScoreResult_BodySubjectFinalCombine> BodySubjectFinalCombineList { get; set; }
        public string TotalScore { get; set; }
        public string GradeScore { get; set; }
    }

    public class GetComponentIB2023SubjectScoreResult_BodySubjectFinalCombine
    {
        public NameValueVm Subject { get; set; }
        public string Teacher { get; set; }
        public GetComponentIB2023SubjectScoreResult_FinalCombineScore FinalCombineScore { get; set; }
    }

    public class GetComponentIB2023SubjectScoreResult_ComponentScore
    {
        public string ComponentShortDesc { get; set; }
        public string Score { set; get; }
    }

    public class GetComponentIB2023SubjectScoreResult_FinalCombineScore
    {
        public string IdSubject { set; get; }
        public string Score { set; get; }
    }

    public class GetComponentIB2023SubjectScoreResult_ComponentData
    {
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdLesson { set; get; }
        public int Semester { set; get; }
        public string IdComponent { set; get; }
        public string ComponentShortDesc { set; get; }
        public string ComponentLongDesc { set; get; }
        public bool ShowGradingAsScore { set; get; }
        public int OrderNoComponent { set; get; }
        public decimal? Score { set; get; }
        public string? Grading { set; get; }
    }

    public class GetComponentIB2023SubjectScoreResult_Footer
    {
        public int NumberOfEmptyFinalGrade { get; set; }
        public string IBGradeTotal { get; set; }
    }
    #endregion
}
