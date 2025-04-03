using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentHistory
{
    public class GetDocumentHistoryRequest : CollectionSchoolRequest
    {
        public string IdDocument { get; set; }
    }
}
