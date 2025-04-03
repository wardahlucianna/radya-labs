using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.SendEmail
{
    public class ResendEmailBLPForParentResult
    {
        public bool IsSuccessAll { get; set; }
        public List<ResendEmailBLPForParentResult_StudentResult> StudentResults { get; set; }
    }

    public class ResendEmailBLPForParentResult_StudentResult
    {
        public string IdStudent { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
