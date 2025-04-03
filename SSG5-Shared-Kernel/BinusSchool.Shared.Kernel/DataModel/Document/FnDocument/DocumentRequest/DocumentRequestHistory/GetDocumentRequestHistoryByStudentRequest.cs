using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class GetDocumentRequestHistoryByStudentRequest
    {
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
        public int? RequestYear { get; set; }
        public DocumentRequestStatusWorkflow? IdDocumentReqStatusWorkflow { get; set; }
    }
}
