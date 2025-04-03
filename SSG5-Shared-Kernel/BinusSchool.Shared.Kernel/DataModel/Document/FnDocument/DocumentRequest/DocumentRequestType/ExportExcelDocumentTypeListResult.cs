using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class ExportExcelDocumentTypeListResult
    {

    }

    public class ExportExcelDocumentTypeListResult_ParamDesc
    {
        public string SchoolDesc { get; set; }
        public string AcademicYear { get; set; }
        public string LevelDesc { get; set; }
        public string GradeDesc { get; set; }
        public string ActiveStatus { get; set; }
        public string VisibleToParent { get; set; }
        public string PaidDocument { get; set; }
    }
}
