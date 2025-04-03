using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApproval
{
    public class SaveDocumentRequestApprovalRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public DocumentRequestApprovalStatus ApprovalStatus { get; set; }
        public string Remarks { get; set; }
    }
}
