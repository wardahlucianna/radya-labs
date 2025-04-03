using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class SavePaymentApprovalRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public DateTime PaymentDate { get; set; }
        public string IdDocumentReqPaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public string SenderAccountName { get; set; }
        public bool VerificationStatus { get; set; }
        public string Remarks { get; set; }
    }
}
