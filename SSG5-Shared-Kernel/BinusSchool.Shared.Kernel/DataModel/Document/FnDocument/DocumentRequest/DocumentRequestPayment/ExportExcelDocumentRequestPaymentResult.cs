using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class ExportExcelDocumentRequestPaymentResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm HomeroomWhenRequestWasMade { get; set; }
        public GetDocumentRequestDetailResult_StudentStatus StudentStatusWhenRequestWasCreated { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public GetDocumentRequestListResult_StatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public decimal TotalAmountReal { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public GetDocumentRequestDetailResult_Approval Approval { get; set; }
        public List<GetDocumentRequestDetailResult_Document> DocumentList { get; set; }
        public GetDocumentRequestDetailResult_Payment Payment { get; set; }
    }

    public class ExportExcelDocumentRequestPaymentResult_ParamDesc
    {
        public string SchoolName { get; set; }
        public string RequestYear { get; set; }
        public string PaymentStatus { get; set; }
        public string DocumentReqStatusWorkflow { get; set; }
        public string SearchKeyword { get; set; }
    }
}
