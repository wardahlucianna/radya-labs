using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetDocumentRequestListWithDetailResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm HomeroomWhenRequestWasMade { get; set; }
        public GetDocumentRequestListWithDetailResult_StudentStatus StudentStatusWhenRequestWasCreated { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public GetDocumentRequestListWithDetailResult_StatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public List<GetDocumentRequestListWithDetailResult_StatusWorkflow> DocumentReqStatusWorkflowHistoryList { get; set; }
        public decimal TotalAmountReal { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public GetDocumentRequestListWithDetailResult_Approval Approval { get; set; }
        public List<GetDocumentRequestListWithDetailResult_Document> DocumentList { get; set; }
        public GetDocumentRequestListWithDetailResult_Payment Payment { get; set; }
    }

    public class GetDocumentRequestListWithDetailResult_StudentStatus : ItemValueVm
    {
        public DateTime? StartDate { get; set; }
    }

    public class GetDocumentRequestListWithDetailResult_StatusWorkflow
    {

        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string Description { get; set; }
        public DateTime StatusDate { get; set; }
        public bool IsOnProcess { get; set; }
        public string Remarks { get; set; }
    }

    public class GetDocumentRequestListWithDetailResult_Approval
    {
        public DocumentRequestApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalRemarks { get; set; }
        public NameValueVm StaffApprover { get; set; }
    }

    public class GetDocumentRequestListWithDetailResult_Document
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string IdDocumentReqType { get; set; }
        public string DocumentName { get; set; }
        public int? NoOfPages { get; set; }
        public int NoOfCopy { get; set; }
        public decimal PriceReal { get; set; }
        public decimal PriceInvoice { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool IsMadeFree { get; set; }
        public ItemValueVm AcademicYearDocument { get; set; }
        public ItemValueVm GradeDocument { get; set; }
        public string HomeroomName { get; set; }
        public ItemValueVm PeriodDocument { get; set; }
        public bool NeedHardCopy { get; set; }
        public bool NeedSoftCopy { get; set; }
        public GetDocumentRequestListWithDetailResult_DocumentIsReady DocumentIsReady { get; set; }
        public List<NameValueVm> PICList { get; set; }
        public List<GetDocumentRequestListWithDetailResult_AdditionalField> AdditionalFieldList { get; set; }
    }

    public class GetDocumentRequestListWithDetailResult_Payment
    {
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public DateTime? EndDatePayment { get; set; }
        public DateTime? PaymentDate { get; set; }
        public GetDocumentRequestListWithDetailResult_PaymentMethod DocumentReqPaymentMethod { get; set; }
        public decimal? PaidAmount { get; set; }
        public string SenderAccountName { get; set; }
        public string AttachmentImageUrl { get; set; }
    }
    public class GetDocumentRequestListWithDetailResult_DocumentIsReady
    {
        public NameValueVm BinusianReceiver { get; set; }
        public DateTime? ReceivedDateByStaff { get; set; }
    }

    public class GetDocumentRequestListWithDetailResult_AdditionalField
    {
        public string IdDocumentReqFormFieldAnswered { get; set; }
        public string QuestionDescription { get; set; }
        public int OrderNo { get; set; }
        public List<string> AnswerTextList { get; set; }
    }
    public class GetDocumentRequestListWithDetailResult_PaymentMethod : ItemValueVm
    {
        public bool? UsingManualVerification { get; set; }
        public bool? IsVirtualAccount { get; set; }
    }
}
