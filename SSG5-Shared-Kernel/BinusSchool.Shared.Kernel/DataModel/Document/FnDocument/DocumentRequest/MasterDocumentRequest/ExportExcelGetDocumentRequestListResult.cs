using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class ExportExcelGetDocumentRequestListResult
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
        public List<GetDocumentRequestListResult_StatusWorkflow> DocumentReqStatusWorkflowHistoryList { get; set; }   // this
        public decimal TotalAmountReal { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public GetDocumentRequestDetailResult_Approval Approval { get; set; }
        public List<GetDocumentRequestDetailResult_Document> DocumentList { get; set; }
        public GetDocumentRequestDetailResult_Payment Payment { get; set; }
    }

    public class ExportExcelGetDocumentRequestListResult_ParamDesc
    {
        public string SchoolName { get; set; }
        public string RequestYear { get; set; }
        public string DocumentTypeName { get; set; }
        public string ApprovalStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string DocumentReqStatusWorkflow { get; set; }
        public string SearchKeyword { get; set; }
    }

    //public class ExportExcelGetDocumentRequestListResult_StatusWorkflow
    //{

    //    public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
    //    public string Description { get; set; }
    //    public DateTime StatusDate { get; set; }
    //    public bool IsOnProcess { get; set; }
    //    public string Remarks { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_Payment
    //{
    //    public decimal TotalAmountInvoice { get; set; }
    //    public DateTime? EndDatePayment { get; set; }
    //    public DateTime? PaymentDate { get; set; }
    //    public ItemValueVm DocumentReqPaymentMethod { get; set; }
    //    public decimal? PaidAmount { get; set; }
    //    public string SenderAccountName { get; set; }
    //    public string AttachmentImageUrl { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_StudentStatus : ItemValueVm
    //{
    //    public DateTime? StartDate { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_Approval
    //{
    //    public bool? ApprovalStatus { get; set; }
    //    public string ApprovalRemarks { get; set; }
    //    public NameValueVm StaffApprover { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_Document
    //{
    //    public string IdDocumentReqApplicantDetail { get; set; }
    //    public string IdDocumentReqType { get; set; }
    //    public string DocumentName { get; set; }
    //    public int? NoOfPages { get; set; }
    //    public int NoOfCopy { get; set; }
    //    public decimal PriceReal { get; set; }
    //    public decimal PriceInvoice { get; set; }
    //    public bool IsMadeFree { get; set; }
    //    public ItemValueVm AcademicYearDocument { get; set; }
    //    public ItemValueVm GradeDocument { get; set; }
    //    public ItemValueVm PeriodDocument { get; set; }
    //    public bool NeedHardCopy { get; set; }
    //    public bool NeedSoftCopy { get; set; }
    //    public ExportExcelGetDocumentRequestListResult_DocumentIsReady DocumentIsReady { get; set; }
    //    public List<NameValueVm> PICList { get; set; }
    //    public List<ExportExcelGetDocumentRequestListResult_AdditionalField> AdditionalFieldList { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_DocumentIsReady
    //{
    //    public NameValueVm BinusianStaffHardCopyReceiver { get; set; }
    //    public DateTime? DateHardCopyReceivedByStaff { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_AdditionalField
    //{
    //    public string IdDocumentReqFormFieldAnswered { get; set; }
    //    public string QuestionDescription { get; set; }
    //    public int OrderNo { get; set; }
    //    public List<string> AnswerTextList { get; set; }
    //}

    //public class ExportExcelGetDocumentRequestListResult_LatestDocumentReqStatusWorkflow
    //{
    //    public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
    //    public string Description { get; set; }
    //    public DateTime StatusDate { get; set; }
    //    public bool IsOnProcess { get; set; }
    //}
}
