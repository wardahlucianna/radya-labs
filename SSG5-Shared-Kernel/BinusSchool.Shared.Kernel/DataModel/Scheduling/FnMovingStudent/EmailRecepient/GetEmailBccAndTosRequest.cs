using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient
{
    public class GetEmailBccAndTosRequest
    {
        public TypeEmailRecepient Type { get; set; }
        public string IdSchool { get;set; }
    }
}
