using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class CancelDocumentRequestByStaffRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public string Remarks { get; set; }
    }
}
