using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class CreateDocumentRequestParentRequest
    {
        public string IdSchool { get; set; }
        public string IdParentApplicant { get; set; }
        public string IdStudent { get; set; }
        public List<CreateDocumentRequestParentRequest_DocumentRequest> DocumentRequestList { get; set; }
        public CreateDocumentRequestParentRequest_Payment Payment { get; set; }
    }

    public class CreateDocumentRequestParentRequest_DocumentRequest
    {
        public string IdDocumentReqType { get; set; }
        public int? NoOfCopy { get; set; }
        public bool IsAcademicDocument { get; set; }
        public string IdGradeDocument { get; set; }
        public string IdPeriodDocument { get; set; }
        public List<CreateDocumentRequestParentRequest_FormAnswer> AdditionalFieldsList { get; set; }
    }

    public class CreateDocumentRequestParentRequest_FormAnswer
    {
        public string IdDocumentReqFormField { get; set; }
        public List<string> IdDocumentReqOptionList { get; set; }
        public string TextValue { get; set; }
    }

    public class CreateDocumentRequestParentRequest_Payment
    {
        public string IdDocumentReqPaymentMethod { get; set; }
    }
    public class CreateDocumentRequestParentRequest_PIC
    {
        public string IdDocumentReqType { get; set; }
        public List<string> IdBinusianList { get; set; }
    }
}
