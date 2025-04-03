using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class CreateDocumentRequestStaffRequest
    {
        public string IdSchool { get; set; }
        public string IdParentApplicant { get; set; }
        public string IdStudent { get; set; }
        public bool CanProcessBeforePaid { get; set; }
        public int EstimationFinishDays { get; set; }
        public List<CreateDocumentRequestStaffRequest_DocumentRequest> DocumentRequestList { get; set; }
        public CreateDocumentRequestStaffRequest_Payment Payment { get; set; }
    }

    public class CreateDocumentRequestStaffRequest_DocumentRequest
    {
        public string IdDocumentReqType { get; set; }
        public int? NoOfCopy { get; set; }
        public bool IsAcademicDocument { get; set; }
        public string IdGradeDocument { get; set; }
        public string IdPeriodDocument { get; set; }
        public List<string> IdBinusianPICList { get; set; }
        public bool IsMakeFree { get; set; }
        public List<CreateDocumentRequestStaffRequest_FormAnswer> AdditionalFieldsList { get; set; }
    }

    public class CreateDocumentRequestStaffRequest_FormAnswer
    {
        public string IdDocumentReqFormField { get; set; }
        public List<string> IdDocumentReqOptionList { get; set;  }
        public string TextValue { get; set; }
    }

    public class CreateDocumentRequestStaffRequest_Payment
    {
        public bool IsPaid { get; set; }
        public string IdDocumentReqPaymentMethod { get; set; }
        public string Remarks { get; set; }
        public string SenderAccountName { get; set; }
        public decimal? PaidAmount { get; set; }
        public DateTime? PaymentDateTime { get; set; }
        public string TransferEvidanceFileName { get; set; }
        public string TransferEvidanceFileUrl { get; set; }
    }
}
