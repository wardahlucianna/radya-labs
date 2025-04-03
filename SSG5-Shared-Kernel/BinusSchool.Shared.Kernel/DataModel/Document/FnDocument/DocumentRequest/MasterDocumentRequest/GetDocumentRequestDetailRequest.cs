using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetDocumentRequestDetailRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public string IdStudent { get; set; }
        public bool IncludePaymentInfo { get; set; }
    }
}
