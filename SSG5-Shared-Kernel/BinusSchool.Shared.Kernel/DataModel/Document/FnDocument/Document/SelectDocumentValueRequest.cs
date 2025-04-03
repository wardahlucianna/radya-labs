using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.Document
{
    public class SelectDocumentValueRequest : CollectionSchoolRequest
    {
        public string IdDocument { get; set; }
    }
}
