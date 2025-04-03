using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.Template
{
    public class AddTemplateRequest 
    {
        public string IdSchoolDocumentCategory { get; set; }
        public string IdApprovalType { get; set; }
        public string IdFormTemplateRefence { get; set; }
        public string IdSchool { get; set; }
        public IEnumerable<string> AcademicYear { get; set; }
        public string Semester { get; set; }
        public IEnumerable<string> Term { get; set; }
        public IEnumerable<string> Level { get; set; }
        public IEnumerable<string> Grade { get; set; }
        public IEnumerable<string> Subject { get; set; }
        public bool IsApprovalForm { get; set; }
        public bool IsMultipleForm { get; set; }
        public string JsonFormElement { get; set; }
        public string JsonSchema { get; set; }
        public AssignmentUserAndRole UserAndRole { get; set; }
    }

    public class AssignmentUserAndRole 
    {
        public string IdRole { get; set; }
        public List<string> IdUser { get; set; } = new List<string>();
    }
}
