using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetOptionsByOptionCategoryRequest
    {
        public string IdDocumentReqOptionCategory { get; set; }
        public List<string> IdDocumentReqOptionChosenList { get; set; }
    }
}
