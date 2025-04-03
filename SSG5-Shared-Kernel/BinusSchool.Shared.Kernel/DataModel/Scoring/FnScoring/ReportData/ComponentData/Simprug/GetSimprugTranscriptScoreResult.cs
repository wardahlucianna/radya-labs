using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugTranscriptScoreResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetSimprugTranscriptScoreResult_SetUsedTemplateForTranscriptScore2023
    {
        public string HtmlOutputTranscriptScore2023 { get; set; }
        public string StartTagTranscriptScore2023 { get; set; }
        public string EndTagTranscriptScore2023 { get; set; }
        public List<GetSimprugTranscriptScoreResult_StudentEnrollment> StudentEnrollment { get; set; }
    }

    public class GetSimprugTranscriptScoreResult_StudentEnrollment
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdLevel { get; set; }
        public string LevelDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public int OrderNumber { get; set; }
        public int Semester { get; set; }
     
    }

    public class GetSimprugTranscriptScoreResult_DPGroup
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public string SubjectDPGroup { get; set; }
        public string ClassIdGenerated { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
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
