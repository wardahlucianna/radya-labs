using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetStudentMasterDataRequest
    {
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
        public string HtmlInput { get; set; }
    }
}
