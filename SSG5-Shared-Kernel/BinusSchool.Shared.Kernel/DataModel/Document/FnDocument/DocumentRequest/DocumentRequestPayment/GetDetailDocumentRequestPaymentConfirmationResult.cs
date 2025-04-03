using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetDetailDocumentRequestPaymentConfirmationResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string IdSchool { get; set; }
        public string RequestNumber { get; set; }
        public ItemValueVm ParentApplicant { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm CurrentHomeroom { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public decimal? TotalAmountInvoice { get; set; }
        public List<GetDetailDocumentRequestPaymentConfirmation_Document> DocumentList { get; set; }
        public bool ParentCanConfirmPayment { get; set; }
        public ItemValueVm PaymentMethod { get; set; }
    }
    public class GetDetailDocumentRequestPaymentConfirmation_Document
    {
        public ItemValueVm Document { get; set; }
        public decimal PricePerCopy { get; set; }
        public int NoOfCopy { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
