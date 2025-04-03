using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetTranscriptScoreCambridgeResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetTranscriptScoreCambridgeResult_SetUsedTemplateForTranscriptScore
    {
        public string HtmlOutputTranscriptScore2023 { get; set; }
        public string StartTagTranscriptScore2023 { get; set; }
        public string EndTagTranscriptScore2023 { get; set; }
        public List<GetTranscriptScoreCambridgeResult_StudentEnrollment> StudentEnrollment { get; set; }
    }

    public class GetTranscriptScoreCambridgeResult_StudentEnrollment
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdLevel { get; set; }
        public string LevelDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public string IdPathway { get; set; }
        public string Pathway { get; set; }
        public int OrderNumber { get; set; }
        public int Semester { get; set; }

    }

    public class GetTranscriptScoreCambridgeResult_Score
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string IdSubjectLevel { get; set; }
        public string SubjectLevel { get; set; }
        public string IdSubjectType { set; get; }
        public string SubjectTypeName { set; get; }
        public int? Semester { get; set; }
        public decimal? Score { get; set; }
        public string? Grading { get; set; }
        public string? PredictedGrade { get; set; }
        public int? OrderNo { get; set; }
    }

    public class GetTranscriptScoreCambridgeResult_TemplateTable
    {
        public string tempAllRowData { get; set; }
        public int Semester { get; set; }
    }
}
