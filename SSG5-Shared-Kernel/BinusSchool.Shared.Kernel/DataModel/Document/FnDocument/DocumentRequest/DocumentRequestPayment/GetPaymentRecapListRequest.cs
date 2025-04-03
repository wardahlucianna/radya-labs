using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetPaymentRecapListRequest
    {
        public string IdSchool { get; set; }
        public DateTime PaymentPeriodStartDate { get; set; }
        public DateTime PaymentPeriodEndDate { get; set; }
    }
}
