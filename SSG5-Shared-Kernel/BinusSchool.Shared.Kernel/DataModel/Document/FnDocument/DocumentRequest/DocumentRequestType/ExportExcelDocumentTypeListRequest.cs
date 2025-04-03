using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class ExportExcelDocumentTypeListRequest
    {
        public string IdSchool { get; set; }
        public string AcademicYearCode { get; set; }
        public string LevelCode { get; set; }
        public string GradeCode { get; set; }
        public bool? ActiveStatus { get; set; }
        public bool? VisibleToParent { get; set; }
        public bool? PaidDocument { get; set; }
    }
}
