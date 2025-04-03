using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class AddStudentGenerateReportProcessRequest
    {
        public List<AddStudentGenerateReportProcess_StudentVm> StudentList { get; set; }    
        public string EmailRequester { get; set; }
    }

    public class AddStudentGenerateReportProcess_StudentVm     
    {
        public string IdReportType { get; set; }
        public string IdPeriod { get; set; }
        public string IdHomeroomStudent { get; set; }
    }
}
