using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugPYPInquiryScoreRequest
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
        public int currPage { get; set; }
        public string templateFooter { get; set; }
        public string ReportScoreTemplate { get; set; }
    }
}
