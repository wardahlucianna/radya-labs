using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class GenerateStudentReportResult
    {
        public string IdStudent { set; get; }
        public string IdHomeroomStudent { set; get; }
        public string IdReportType { set; get; }
        public bool IsCompleteScoring { set; get; }
        public string UnCompleteScoringDetail { set; get; }
        public string LinkPreview { set; get; }
        public string Generated { set; get; }
    }

    public class GenerateStudentReportResult_TranReportGenerateProcess
    {
        public string IdReport { get; set; }
        public string RequestID { get; set; }
        public string IdReportGenerateProcess { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public string IdLevel { get; set; }
        public string IdPeriod { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public string ClassroomDesc { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdReportType { get; set; }
        public string ReportTypeMappings { get; set; }
        public string RequesterEmail { get; set; }
        public string UserIn { get; set; }
        public string? GenerateStatus { set; get; }
        public GenerateStatus? ProcessStatus { set; get; }
        public string? Url { set; get; }
    }

    public class GenerateStudentReportResult_TemplateReport
    {
        public string IdReportTemplate { get; set; }
        public string TemplateName { get; set; }
        public string? Title { get; set; }
        public string? TOCGroup { get; set; }
        public int? PageNumber { get; set; }
        public string? Categories { get; set; }
        public bool IsCategoriesYearly { get; set; }
        public bool IsCategoriesSemester { get; set; }
        public bool IsCategoriesTerm { get; set; }
    }
}
