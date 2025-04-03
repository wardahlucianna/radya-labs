using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.School.FnSchool;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class GetDocumentRequestHistoryByStudentResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string IdDocumentReqApplicantEncrypted { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm Student { get; set; }
        public List<GetDocumentRequestHistoryByStudentResult_Document> DocumentList { get; set; }
        public decimal TotalAmountReal { get; set; }
        public GetDocumentRequestHistoryByStudentResult_PaymentStatus PaymentStatus { get; set; }
        public GetDocumentRequestHistoryByStudentResult_StatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public List<GetDocumentRequestHistoryByStudentResult_StatusWorkflow> DocumentReqStatusWorkflowHistoryList { get; set; }
        public bool CanCancelRequest { get; set; }
        public bool CanConfirmPayment { get; set; }
    }
    public class GetDocumentRequestHistoryByStudentResult_Document : ItemValueVm
    {
        public string AttachmentUrl { get; set; }
    }

    public class GetDocumentRequestHistoryByStudentResult_StatusWorkflow
    {

        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string Description { get; set; }
        public DateTime StatusDate { get; set; }
        public bool IsOnProcess { get; set; }
        public string Remarks { get; set; }
        public GetDocumentRequestHistoryByStudentResult_StatusWorkflowDetail workflowDetail { get; set; }
    }
    public class GetDocumentRequestHistoryByStudentResult_StatusWorkflowDetail
    {
        public string ApprovalStatus { get; set; }
        public DateTime? ScheduleCollectionDateStart { get; set; }
        public DateTime? ScheduleCollectionDateEnd { get; set; }
        public string Notes { get; set; }
        public string Venue { get; set; }
        public string CollectedBy { get; set; }
        public DateTime? CollectedDate { get; set; }
    }
    public class GetDocumentRequestHistoryByStudentResult_PaymentStatus
    {

        public PaymentStatus PaymentStatus { get; set; }
        public string Description { get; set; }
    }
}
