using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetTOCReportResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetTOCReportResult_ReportTOC
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string IdLevel { get; set; }
        public string LevelCode { get; set; }
        public string IndonesianLevel { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public int Semester { get; set; }
        public string ReportTitle { get; set; }
        public int? PageNumber { get; set; }
        public string TOCGroup { get; set; }
    }
}
