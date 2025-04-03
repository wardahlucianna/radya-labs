using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.SendEmail
{
    public class SendEmailConcentFormForParentRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
    }
}
