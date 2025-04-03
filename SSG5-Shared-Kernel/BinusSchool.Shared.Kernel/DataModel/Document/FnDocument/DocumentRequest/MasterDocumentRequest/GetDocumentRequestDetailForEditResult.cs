using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetDocumentRequestDetailForEditResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public DateTime? StartOnProcessDate { get; set; }
        public int EstimationFinishDays { get; set; }
        public string IdSchool { get; set; }
        public ItemValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public List<GetDocumentRequestDetailForEditResult_Document> DocumentList { get; set; }
        public GetDocumentRequestDetailForEditResult_Payment Payment { get; set; }
    }
    public class GetDocumentRequestDetailForEditResult_Document
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string IdDocumentReqType { get; set; }
        public string DocumentName { get; set; }
        public string DocumentTypeDescription { get; set; }
        public int? NoOfPages { get; set; }
        public int NoOfCopy { get; set; }
        public decimal PriceReal { get; set; }
        public decimal PriceInvoice { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool IsMadeFree { get; set; }
        public ItemValueVm AcademicYearDocument { get; set; }
        public ItemValueVm GradeDocument { get; set; }
        public string HomeroomNameDocument { get; set; }
        public ItemValueVm AcademicYearAndGradeDocument { get; set; }
        public ItemValueVm PeriodDocument { get; set; }
        public int EstimatedProcessDays { get; set; }
        public bool NeedHardCopy { get; set; }
        public bool NeedSoftCopy { get; set; }
        public List<NameValueVm> PICList { get; set; }
        public List<GetDocumentRequestDetailForEditResult_AdditionalField> AdditionalFieldList { get; set; }
    }

    public class GetDocumentRequestDetailForEditResult_AdditionalField
    {
        public string IdDocumentReqFormFieldAnswered { get; set; }
        public ItemValueVm FieldType { get; set; }
        public bool HasOption { get; set; }
        public string QuestionDescription { get; set; }
        public int OrderNo { get; set; }
        public bool IsRequired { get; set; }
        public List<GetDocumentRequestDetailForEditResult_AdditionalFieldOptions> Options { get; set; }
        public string TextValue { get; set; }
        public List<string> IdDocumentReqOptionAnsweredList { get; set; }
    }

    public class GetDocumentRequestDetailForEditResult_AdditionalFieldOptions
    {
        public string IdDocumentReqOption { get; set; }
        public string OptionDescription { get; set; }
    }

    public class GetDocumentRequestDetailForEditResult_Payment
    {
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public DateTime? EndDatePayment { get; set; }
        public DateTime? PaymentDate { get; set; }
        public GetDocumentRequestDetailForEditResult_PaymentMethod DocumentReqPaymentMethod { get; set; }
        public decimal? PaidAmount { get; set; }
        public string SenderAccountName { get; set; }
        public string AttachmentImageUrl { get; set; }
    }

    public class GetDocumentRequestDetailForEditResult_PaymentMethod : ItemValueVm
    {
        public bool? UsingManualVerification { get; set; }
        public bool? IsVirtualAccount { get; set; }
    }
}
