using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.ConsentForm
{
    public class GetStudentConsentFormRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdParent { get; set; }
    }
}
