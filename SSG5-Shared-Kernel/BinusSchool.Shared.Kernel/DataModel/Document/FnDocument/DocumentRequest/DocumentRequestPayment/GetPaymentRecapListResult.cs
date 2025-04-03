using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetPaymentRecapListResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm HomeroomWhenRequestWasMade { get; set; }
        public GetPaymentRecapListResult_StudentStatus StudentStatusWhenRequestWasCreated { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public GetPaymentRecapListResult_StatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public decimal TotalAmountReal { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public GetPaymentRecapListResult_Approval Approval { get; set; }
        public List<GetPaymentRecapListResult_Document> DocumentList { get; set; }
        public GetPaymentRecapListResult_Payment Payment { get; set; }
    }

    public class GetPaymentRecapListResult_StudentStatus : ItemValueVm
    {
        public DateTime? StartDate { get; set; }
    }

    public class GetPaymentRecapListResult_StatusWorkflow
    {

        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string Description { get; set; }
        public DateTime StatusDate { get; set; }
        public bool IsOnProcess { get; set; }
    }

    public class GetPaymentRecapListResult_Approval
    {
        public DocumentRequestApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalRemarks { get; set; }
        public NameValueVm StaffApprover { get; set; }
    }

    public class GetPaymentRecapListResult_Document
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
        public bool NeedHardCopy { get; set; }
        public bool NeedSoftCopy { get; set; }
        public GetPaymentRecapListResult_DocumentIsReady DocumentIsReady { get; set; }
    }

    public class GetPaymentRecapListResult_Payment
    {
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public DateTime? EndDatePayment { get; set; }
        public DateTime? PaymentDate { get; set; }
        public GetPaymentRecapListResult_PaymentMethod DocumentReqPaymentMethod { get; set; }
        public decimal? PaidAmount { get; set; }
        public string SenderAccountName { get; set; }
        public string AttachmentImageUrl { get; set; }
        public DateTime? PaymentVerificationDate { get; set; }
    }
    public class GetPaymentRecapListResult_DocumentIsReady
    {
        public DateTime? ReceivedDateByStaff { get; set; }
    }

    public class GetPaymentRecapListResult_PaymentMethod : ItemValueVm
    {
        public bool? UsingManualVerification { get; set; }
        public bool? IsVirtualAccount { get; set; }
    }
}
