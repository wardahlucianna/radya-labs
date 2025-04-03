using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ReportIssueDate
{
    public class GetReportIssueDateListRequest : CollectionRequest
    {
        public string IdAcademicYear { set; get; }
        public string? IdLevel { set; get; }
        public string? IdGrade { set; get; }
        public int? Semester { set; get; }
        public string? IdPeriod { set; get; }
        public string? IdReportType { set; get; }
    }
}
