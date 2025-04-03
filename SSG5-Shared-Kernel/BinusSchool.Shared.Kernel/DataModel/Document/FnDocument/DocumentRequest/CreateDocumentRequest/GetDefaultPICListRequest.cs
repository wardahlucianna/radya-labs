using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDefaultPICListRequest : CollectionRequest
    {
        public string IdDocumentReqType { get; set; }
    }
}
