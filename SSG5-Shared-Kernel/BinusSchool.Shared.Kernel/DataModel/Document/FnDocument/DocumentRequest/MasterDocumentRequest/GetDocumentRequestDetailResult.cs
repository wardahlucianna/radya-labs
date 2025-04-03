using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetDocumentRequestDetailResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public string IdSchool { get; set; }
        public ItemValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public GetDocumentRequestDetailResult_StudentStatus StudentStatusWhenRequestWasCreated { get; set; }
        public CodeWithIdVm LevelWhenRequestWasMade { get; set; }
        public ItemValueVm HomeroomWhenRequestWasMade { get; set; }
        public GetDocumentRequestDetailResult_LatestDocumentReqStatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public GetDocumentRequestDetailResult_Approval Approval { get; set; }
        public List<GetDocumentRequestDetailResult_Document> DocumentList { get; set; }
        public GetDocumentRequestDetailResult_Payment Payment { get; set; }
        public GetDocumentRequestDetailResult_Configuration Configuration { get; set; }
        public GetDocumentRequestDetailResult_CollectionInfo CollectionInfo { get; set; }
    }

    public class GetDocumentRequestDetailResult_Configuration
    {
        public bool CanApproveDocumentRequest { get; set; }
        public bool CanVerifyPayment { get; set; }
        public bool CanFinishRequest { get; set; }
        public bool EnableDocumentIsReadyButton { get; set; }
        public bool SoftCopyDocumentOnly { get; set; }
        public bool CanDeleteSoftCopy { get; set; }
    }
    public class GetDocumentRequestDetailResult_CollectionInfo
    {
        public DateTime FinishDate { get; set; }
        public DateTime? ScheduleCollectionDateStart { get; set; }
        public DateTime? ScheduleCollectionDateEnd { get; set; }
        public string Remarks { get; set; }
        public ItemValueVm Venue { get; set; }
        public string CollectedBy { get; set; }
        public DateTime? CollectedDate { get; set; }
    }


    public class GetDocumentRequestDetailResult_Payment
    {
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public DateTime? EndDatePayment { get; set; }
        public DateTime? PaymentDate { get; set; }
        public GetDocumentRequestDetailResult_PaymentMethod DocumentReqPaymentMethod { get; set; }
        public decimal? PaidAmount { get; set; }
        public string SenderAccountName { get; set; }
        public string AttachmentImageUrl { get; set; }
    }
    public class GetDocumentRequestDetailResult_PaymentMethod : ItemValueVm
    {
        public bool? UsingManualVerification { get; set; }
        public bool? IsVirtualAccount { get; set; }
    }

    public class GetDocumentRequestDetailResult_StudentStatus : ItemValueVm
    {
        public DateTime? StartDate { get; set; }
    }

    public class GetDocumentRequestDetailResult_Approval
    {
        public DocumentRequestApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalRemarks { get; set; }
        public NameValueVm StaffApprover { get; set; }
    }

    public class GetDocumentRequestDetailResult_Document
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
        public string HomeroomNameDocument { get; set; }
        public ItemValueVm PeriodDocument { get; set; }
        public bool NeedHardCopy { get; set; }
        public bool NeedSoftCopy { get; set; }
        public GetDocumentRequestDetailResult_DocumentIsReady DocumentIsReady { get; set; }
        public List<NameValueVm> PICList { get; set; }
        public List<GetDocumentRequestDetailResult_AdditionalField> AdditionalFieldList { get; set; }
    }

    public class GetDocumentRequestDetailResult_DocumentIsReady
    {
        public NameValueVm BinusianReceiver { get; set; }
        public DateTime? ReceivedDateByStaff { get; set; }
    }

    public class GetDocumentRequestDetailResult_AdditionalField
    {
        public string IdDocumentReqFormFieldAnswered { get; set; }
        public string QuestionDescription { get; set; }
        public int OrderNo { get; set; }
        public List<string> AnswerTextList { get; set; }
    }

    public class GetDocumentRequestDetailResult_LatestDocumentReqStatusWorkflow
    {
        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public DateTime StatusDate { get; set; }
        public bool IsOnProcess { get; set; }
    }
}
