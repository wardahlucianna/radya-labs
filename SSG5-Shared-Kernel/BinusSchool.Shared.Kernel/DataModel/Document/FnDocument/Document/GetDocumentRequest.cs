using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Document
{
    public class GetDocumentRequest : CollectionRequest
    {
        public GetDocumentRequest()
        {
            IdCategory = IdAcadyear = IdTerm = IdSemester = IdLevel = IdGrade = IdSubject = IdApproval = string.Empty;
        }
        
        public string IdCategory { get; set; }
        public string IdAcadyear { get; set; }
        public string IdTerm { get; set; }
        public string IdSemester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdApproval { get; set; }
        public string IdDocument { get; set; }
    }
}
