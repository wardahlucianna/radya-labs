using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class UpdateDocumentTypeStatusRequest
    {
        public string IdDocumentReqType { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
