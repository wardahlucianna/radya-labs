using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class GetStudentForFilterGenerateReportRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string? IdHomeroom { set; get; }
        public int? IdStudentStatus { get; set; }
    }
}
