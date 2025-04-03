using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetKurMerReportResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetKurMerReportResult_Teacher
    {
        public string IdLesson { get; set; }
        public string IdSubject { get; set; }
        public string NickName { get; set; }
        public string TeacherNameWithTitle { get; set; }
        public string TeacherName { get; set; }
    }

    public class GetKurMerReportResult_Score
    {
        public string StudentName { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public int? IdScoreLegend { get; set; }
        public int Semester { get; set; }
        public decimal? Score { get; set; }
        public string Grading { get; set; }
    }

    public class GetKurMerReportResult_StudentTekken
    {
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public int? IdScoreLegend { get; set; }
        public string SubjectName { get; set; }
        public string SubjectNameNatCur { get; set; }
        public string SubjectDPGroup { get; set; }
        public int Semester { get; set; }
        public int? OrderNo { get; set; }
    }

    public class GetKurMerReportResult_ScoreLegendDetail
    {
        public int? IdScoreLegend { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string ConvertScore { get; set; }
    }

    public class GetKurMerReportResult_ComponentScoreGradeComment
    {
        public string IdSubject { get; set; }
        public int Semester { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public string Description { get; set; }
    }

    public class GetKurMerReportResult_DPGroup
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public string SubjectDPGroup { get; set; }
        public string ClassIdGenerated { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string SubjectNameNatCur { get; set; }
        public string IdSubjectLevel { get; set; }
        public string SubjectLevel { get; set; }
        public int? Semester { get; set; }
        public decimal? Score { get; set; }
        public string? Grading { get; set; }
        public string? PredictedGrade { get; set; }
        public int? OrderNo { get; set; }
        public int? OrderNoDPGroup { get; set; }
    }
}
