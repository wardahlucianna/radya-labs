using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ReportIssueDate
{
    public class AddReportIssueDateRequest
    {
        public string IdAcademicYear { set; get; }
        public string Term { set; get; }
        public string IdReportType { set; get; }
        public DateTime IssueDate { set; get; }
        public List<string> Grades { set; get; }
    }
}

