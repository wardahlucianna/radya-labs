using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetExcelReportStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUserStudent { get; set; }
        public string Search { get; set; }
        public string IdHomeroom { get; set; }


    }
}
