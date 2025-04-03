using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf
{
    public class ConvertBnsReportToPdfRequest
    {
        public bool IsDraft { get; set; }
        public string IdReportTemplate { get; set; }
        public string IdReportType { get; set; }
        public string TemplateName { get; set; }
        public string IdSchool { get; set; }
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdPeriod { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
    }
}
