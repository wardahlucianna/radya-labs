using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetDocumentRequestListResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm HomeroomWhenRequestWasMade { get; set; }
        public ItemValueVm StudentStatusWhenRequestWasCreated { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public List<ItemValueVm> DocumentList { get; set; }
        public GetDocumentRequestListResult_StatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public List<GetDocumentRequestListResult_StatusWorkflow> DocumentReqStatusWorkflowHistoryList { get; set; }
        public decimal TotalAmountReal { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DocumentRequestApprovalStatus ApprovalStatus { get; set; }
        public GetDocumentRequestListResult_Configuration Configuration { get; set; }
    }

    public class GetDocumentRequestListResult_Configuration
    {
        public bool CanEditRequest { get; set; }
        public bool CanCancelRequest { get; set; }
        public bool CanFinishRequest { get; set; }
        public bool CanApproveDocumentRequest { get; set; }
        public bool CanManageSoftCopy { get; set; }
    }

    public class GetDocumentRequestListResult_StatusWorkflow
    {

        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string Description { get; set; }
        public DateTime StatusDate { get; set; }
        public bool IsOnProcess { get; set; }
        public string Remarks { get; set; }
    }
}
