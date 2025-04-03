using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.SendEmail
{
    public class SendEmailInvoiceToParentRequest
    {
        public string IdStudent { get; set; }
        public List<string> IdTransactionList { get; set; }
    }
}
