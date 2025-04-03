using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Template
{
    public class GetTemplateRequest : CollectionRequest
    {
        public string IdSchoolDocumentCategory { get; set; }
        public string Acadyear { get; set; }
        public string Level { get; set; }
    }
}
