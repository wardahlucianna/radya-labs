﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.SendEmail
{
    public class SendEmailBulkInvoiceToParentRequest
    {
        public List<string> IdTransactionList { get; set; }
        public bool? IsResendEmail { get; set; }
    }
}
