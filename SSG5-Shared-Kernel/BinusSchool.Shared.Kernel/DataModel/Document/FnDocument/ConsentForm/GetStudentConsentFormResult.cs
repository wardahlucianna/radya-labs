using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.ConsentForm
{
    public class GetStudentConsentFormResult
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroomStudent { get; set; }        
        public string IdGrade { get; set; }
        public string Class { get; set; }        
        public string BLPStatus { get; set; }
        public string Group { get; set; }
    }
}
