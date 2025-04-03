using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.Document
{
    public class ApprovalDocumentRequest
    {
        public string IdApprovalTask { get; set; }
        public string IdDocuemnt { get; set; }
        public string IdFormState { get; set; }
        public ApprovalAction Action { get; set; }
        public string Comment { get; set; }
    }
}
