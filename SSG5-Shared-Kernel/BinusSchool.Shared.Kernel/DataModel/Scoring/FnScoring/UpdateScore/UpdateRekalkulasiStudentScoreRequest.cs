using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore
{
    public class UpdateRekalkulasiStudentScoreRequest
    {
        public bool isFromEntryScore { set; get; }
        public List<UpdateRekalkulasiStudentScore_StudentComponentVm> StudentUpdates { set; get; }
    }

    public class UpdateRekalkulasiStudentScore_StudentComponentVm
    {
        public string IdStudent { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
    }

    public class UpdateRekalkulasiStudentScore_SubjectScoreFinalCombinedVm
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public string Grading { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_ComponentScoreWeight
    {
        public string IdComponent { get; set; }
        public decimal Weight { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_SubjectScoreWeight
    {
        public string IdSubjectScoreSetting { get; set; }
        public decimal Weight { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_SemesterScoreWeight
    { 
        public string IdSubjectScoreSemesterSetting { get; set; }
        public decimal Weight { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_TrScore
    {
        public string IdSubComponentCounter { get; set; }
        public string IdSubComponent { get; set; }
        public decimal Score { get; set; }
        public decimal SubComponentCounterWeight { get; set; }
        public bool IsAvg { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_TrSubComponentScore
    {
        public string IdSubComponent { get; set; }
        public decimal Score { get; set; }
        public decimal? ScoreDecimal5 { get; set; }
        public decimal SubComponentWeight { get; set; }
        public bool IsAvg { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_TrComponentScore
    {
        public string IdComponent { get; set; }
        public decimal Score { get; set; }
        public decimal? ScoreDecimal5 { get; set; }
        public decimal ComponentWeight { get; set; }
        public decimal SubComponentMaxScore { get; set; }
        public decimal FinalMaxScore { get; set; }
        //public DateTime? OrderDate { get; set; }
        public int? DecimalPlaces { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_TrSubjectScore
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public string IdSubjectScoreSemesterSetting { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public string IdSubjectScore { get; set; }
        public decimal Score { get; set; }
        public decimal? ScoreDecimal5 { get; set; }
        public decimal SubjectScoreWeight { get; set; }

        //public DateTime? OrderDate { get; set; }
        public int? DecimalPlaces { get; set; }
        public string IdScoreLegend { get; set; }
        public List<UpdateRekalkulasiStudentScore_SubjectScoreLegen> SubjectScoreLegendList { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_SubjectScoreLegen
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string Grade { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_TrSubjectScoreSemester
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public string IdSubjectScoreSemesterSetting { get; set; }
        public int Semester { get; set; }
        public decimal Score { get; set; }
        public decimal SemesterScoreWeight { get; set; }
        //public DateTime? OrderDate { get; set; }
        public int? DecimalPlaces { get; set; }
    }

    public class UpdateRekalkulasiStudentScore_UnusualScore
    {
        public decimal ErrorScore { set; get; }
        public decimal ZeroScore { set; get; }
        public List<UpdateRekalkulasiStudentScore_MinusScore> MinusScore { set; get; }
    }

    public class UpdateRekalkulasiStudentScore_MinusScore
    {
        public decimal Key { set; get; }
        public string Desc { set; get; }
        public string Category { set; get; }
    }
}
