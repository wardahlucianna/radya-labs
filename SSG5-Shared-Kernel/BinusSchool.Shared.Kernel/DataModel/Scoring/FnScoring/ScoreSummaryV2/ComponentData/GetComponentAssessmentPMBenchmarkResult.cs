using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentAssessmentPMBenchmarkResult
    {
        public GetComponentAssessmentPMBenchmarkResult_Header Header { get; set; }
        public List<GetComponentAssessmentPMBenchmarkResult_Body> Body { get; set; }
        
    }

    public class GetComponentAssessmentPMBenchmarkResult_Header
    {
        public List<string> Semester1 { get; set; }
        public List<string> Semester2 { get; set; }

    }

    public class GetComponentAssessmentPMBenchmarkResult_Body
    {
        public string Grade { get; set; }
        public string TeacherName { get; set; }
        public List<GetComponentAssessmentPMBenchmarkResult_ComponentScore> Semester1Score { get; set; }
        public List<GetComponentAssessmentPMBenchmarkResult_ComponentScore> Semester2Score { get; set; }
    }

    public class GetComponentAssessmentPMBenchmarkResult_ComponentScore
    {
        public string ComponentDescription { get; set; }
        public string Score { get; set; }
    }

    public class GetComponentAssessmentPMBenchmarkResult_HomeroomStudent
    {
        public string IdAcademicYear { get; set; }
        public int AcademicYearCodeInt { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public string HomeroomDesc { get; set; }
        public int OrderNumbeAcademicYear { get; set; }
        public int OrderNumbeLevel { get; set; }
        public int Semester { get; set; }
    }

    public class GetComponentAssessmentPMBenchmarkResult_StudentComponentScore
    {
        public string IdAcademicYear { get; set; }
        public string IdAcademicYearInt { get; set; }
        public string IdAssessmentSetting { set; get; }
        public string IdPeriod { get; set; }
        public string PeriodDescription { get; set; }
        public string IdAssessmentComponentSetting { get; set; }
        public string ComponentDescription { set; get; }
        public string IdScoreOption { set; get; }
        public int Semester { set; get; }
        public int OrderNumber { set; get; }
        public string IdGrade { set; get; }
        public string GradeDescription { set; get; }
        public string? IdAsessmentScore { set; get; }
        public string? Score { set; get; }
        public int? KeyScore { get; set; }
        public string? IdLesson { set; get; }
        public string IdHomeroom { get; set; }
        public string TeacherName { get; set; }
    }

    public class GetComponentAssessmentPMBenchmarkResult_data
    {
        public string AcademicYears { get; set; }
        public List<GetComponentAssessmentPMBenchmarkResult_StudentComponentScore> Semester1 { get; set; }
        public List<GetComponentAssessmentPMBenchmarkResult_StudentComponentScore> Semester2 { get; set; }

    }

}
