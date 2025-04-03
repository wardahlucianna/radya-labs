using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class GetListDocumentRequestYearRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
    }
}
