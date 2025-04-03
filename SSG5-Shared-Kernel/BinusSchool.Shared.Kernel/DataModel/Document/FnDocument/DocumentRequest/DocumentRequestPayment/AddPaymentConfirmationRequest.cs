using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class AddPaymentConfirmationRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string IdDocumentReqPaymentMethod { get; set; }
        public string SenderAccountName { get; set; }
    }
}
