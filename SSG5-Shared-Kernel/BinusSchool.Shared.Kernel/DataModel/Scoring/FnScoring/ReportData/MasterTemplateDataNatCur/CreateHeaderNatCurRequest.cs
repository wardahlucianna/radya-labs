using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.School;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur
{
    public class CreateHeaderNatCurRequest
    {
        public string IdReportTemplate { get; set; }
        public string IdReportType { get; set; }
        public string TemplateName { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
