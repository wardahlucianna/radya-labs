using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetOptionCategoryByFieldTypeResult
    {
        public string IdDocumentReqOptionCategory { get; set; }
        public string CategoryDescription { get; set; }
        public bool IsDefaultImportData { get; set; }
    }
}
