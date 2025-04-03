using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class SaveSoftCopyDocumentRequest
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public bool ShowToParent { get; set; }
    }
}
