using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail
{
    public class SendEmailDeleteNotPaidExtracurricularToParentRequest
    {
        public NameValueVm School { get; set; }
        public NameValueVm Extracurricular { get; set; }
        public NameValueVm Student { get; set; }
        public int DueHourPayment { get; set; }
    }
}
