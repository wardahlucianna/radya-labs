using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.SendEmail
{
    public class ResendEmailBLPForParentRequest
    {
        public string IdSchool { get; set; }
        public string IdSurveyPeriod { get; set; }
        public string IdClearanceWeekPeriod { get; set; }
        public string IdStudent { get; set; }
    }
}
