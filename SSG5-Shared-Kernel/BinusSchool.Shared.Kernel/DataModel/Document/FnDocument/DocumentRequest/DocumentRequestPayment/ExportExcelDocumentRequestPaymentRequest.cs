using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class ExportExcelDocumentRequestPaymentRequest
    {
        public string IdSchool { get; set; }
        public int RequestYear { get; set; }   // 1 = within last one year
        public PaymentStatus? PaymentStatus { get; set; }
        public DocumentRequestStatusWorkflow? IdDocumentReqStatusWorkflow { get; set; }
        public string SearchQuery { get; set; }     // StudentID, StudentName, RequestNumber
    }
}
