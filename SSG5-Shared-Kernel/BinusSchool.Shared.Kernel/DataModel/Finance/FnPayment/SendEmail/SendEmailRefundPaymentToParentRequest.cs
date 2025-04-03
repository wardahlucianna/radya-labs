using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.SendEmail
{
    public class SendEmailRefundPaymentToParentRequest
    {
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
        public string IdRefundStudent { get; set; }
        //public string IdRefundPayment { get; set; }
        //public string IdHomeroomStudent { get; set; }
    }
}
