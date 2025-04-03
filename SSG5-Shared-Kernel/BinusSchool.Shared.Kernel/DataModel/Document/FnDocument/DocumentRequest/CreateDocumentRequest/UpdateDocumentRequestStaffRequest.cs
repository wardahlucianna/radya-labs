using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class UpdateDocumentRequestStaffRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public string IdParentApplicant { get; set; }
        public int EstimationFinishDays { get; set; }
        public List<UpdateDocumentRequestStaffRequest_DocumentRequest> DocumentRequestList { get; set; }
    }

    public class UpdateDocumentRequestStaffRequest_DocumentRequest
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string IdPeriodDocument { get; set; }
        public List<string> IdBinusianPICList { get; set; }
        public List<UpdateDocumentRequestStaffRequest_FormAnswer> AdditionalFieldsList { get; set; }
    }

    public class UpdateDocumentRequestStaffRequest_FormAnswer
    {
        public string IdDocumentReqFormFieldAnswered { get; set; }
        public List<string> IdDocumentReqOptionList { get; set; }
        public string TextValue { get; set; }
    }
}
