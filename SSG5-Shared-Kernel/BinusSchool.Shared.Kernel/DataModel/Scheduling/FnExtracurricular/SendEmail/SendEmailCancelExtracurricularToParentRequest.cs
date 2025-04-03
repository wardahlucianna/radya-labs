using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail
{
    public class SendEmailCancelExtracurricularToParentRequest
    {
        public NameValueVm School { get; set; }
        public NameValueVm Extracurricular { get; set; }
        public NameValueVm Student { get; set; }
    }
}
