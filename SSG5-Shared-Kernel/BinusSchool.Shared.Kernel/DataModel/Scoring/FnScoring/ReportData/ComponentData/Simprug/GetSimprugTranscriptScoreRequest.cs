using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugTranscriptScoreRequest
    {
        //public string Code { get; set; }
        //public string IdReportTemplate { get; set; }
        public string IdReportType { get; set; }
        //public string TemplateName { get; set; }
        public string SubjectTypeTag { get; set; }
        //public string IdReportScoreViewTemplate { get; set; }
        public string ReportViewTemplate { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
