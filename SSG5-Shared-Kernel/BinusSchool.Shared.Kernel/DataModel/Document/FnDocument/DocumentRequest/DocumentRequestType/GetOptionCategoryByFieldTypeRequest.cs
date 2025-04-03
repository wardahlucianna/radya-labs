using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetOptionCategoryByFieldTypeRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdDocumentReqFieldType { get; set; }
    }
}
