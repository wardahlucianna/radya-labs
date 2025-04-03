using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDocumentTypeByCategoryRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool RequestByParent { get; set; }

        // Academic Document
        public string IdGrade { get; set; }
    }
}
