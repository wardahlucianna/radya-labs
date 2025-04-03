using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ExportPDF
{
    public class ExportPdfForParentScoreSummaryViewRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
    }
}
