using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDocument
{
    public class GetDocumentByStudentResult
    {
        public string MarketingDocumentLink { get; set; }
        public bool MarketingOnly { get; set; }
        public string DocumentStudentID { get; set; }        
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
        public decimal FileSize { get; set; }
        public string DocumentID { get; set; }
        public string FileName { get; set; }
        public string Comment { get; set; }
        public bool IsStudentView { get; set; }
    }
}
