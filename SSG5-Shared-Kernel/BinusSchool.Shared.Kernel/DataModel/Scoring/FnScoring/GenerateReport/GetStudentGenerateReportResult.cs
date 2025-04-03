using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class GetStudentGenerateReportResult
    {
        public int totalStudent { set; get; }
        public int totalGenerated { set; get; }
        public int totalOnProgress { set; get; }
        public int totalNotGenerated { set; get; }       
        public List<GetStudentGenerateReport_StudentReportVm> ReportList { set; get; }   
    }

    public class GetStudentGenerateReportResult_StudentData
    {
        public string IdHomeroomStudent { set; get; }
        public string GradeCode { set; get; }
        public string ClassroomCode { set; get; }
        public string IdStudent { set; get; }
        public string StudentFirstName { set; get; }
        public string StudentLastName { set; get; }
        public string? IdReportType { set; get; }
        public string? ReportTypeName { set; get; }
        public GenerateStatus? ProcessStatus { set; get; }
        public DateTime? DateStart { set; get; }
        public DateTime? DateEnd { set; get; }
        public bool? IsAgent { set; get; }
        public string? IdPeriod { set; get; }
        public string? IdReport { set; get; }
        public string? RequestID { set; get; }
        public string? RequesterEmail { set; get; }
        public string? Remarks { set; get; }
        public DateTime? DateIn { set; get; }
        public string? GenerateStatus { set; get; }
        public string? Url { set; get; }
        public DateTime? ReportDateIn { set; get; }
        public DateTime? ReportDateUp { set; get; }
    }

    public class GetStudentGenerateReport_StudentReportVm
    {
        public string IdHomeroomStudent { set; get; }
        public string Homeroom { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public int? IdStudentStatus { set; get; }
        public string? StudentStatus { set; get; }
        public string? IdReportType { set; get; }
        public string? ReportTypeName { set; get; }
        public bool? IsCompleteScoring { set; get; }
        public string? UnCompleteScoringDetail { set; get; }
        public string? LinkPreview { set; get; }
        public string? Generated { set; get; }
        public string? GeneratedDate { set; get; }
    }

    public class GetStudentGenerateReport_StudentStatus
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdPeriod { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public ItemValueVm StudentStatus { get; set; }
        public bool IsExitStudent { get; set; }
    }
}                                      
