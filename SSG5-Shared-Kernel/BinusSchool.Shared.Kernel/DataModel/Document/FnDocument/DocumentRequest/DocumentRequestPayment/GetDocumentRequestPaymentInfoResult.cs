using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetDocumentRequestPaymentInfoResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusDescription { get; set; }
        public decimal TotalAmountReal { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public bool? UsingManualVerification { get; set; }
        public bool? IsVirtualAccount { get; set; }
        public DateTime? StartDatePayment { get; set; }
        public DateTime? EndDatePayment { get; set; }
        public ItemValueVm DocumentReqPaymentMethod { get; set; }
        public bool? HasConfirmPayment { get; set; }
        public decimal? PaidAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? PaymentVerificationDate { get; set; }
        public string AttachmentImageUrl { get; set; }
        public string SenderAccountName { get; set; }
    }
}
