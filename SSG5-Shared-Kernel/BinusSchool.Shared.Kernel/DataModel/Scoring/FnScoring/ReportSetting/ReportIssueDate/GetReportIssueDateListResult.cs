using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ReportIssueDate
{
    public class GetReportIssueDateListResult : CodeWithIdVm
    {
        public string AcademicYear { set; get; }
        public string Level { set; get; }
        public string Grade { set; get; }
        public string Semester { set; get; }
        public string Term { set; get; }
        public string IdReportIssueDate { set; get; }
        public string ReportType { set; get; }
        public string IssueDate { set; get; }
    }
}
