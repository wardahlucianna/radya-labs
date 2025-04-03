using System.Collections.Generic;

namespace BinusSchool.Domain.NoEntities.FormBuilder
{
    public class Form : UniqueNoEntity
    {
        public object FormBuilder { get; set; }
        public Header Header { get; set; }
        public Assign Assign { get; set; }
        public List<ReferenceDocument> ReferenceDocument { get; set; }
        public List<History> History { get; set; }
        public object JsonSchema { get; set; }
    }

    public class Header
    {
        public List<string> AcademicYear { get; set; }
        public List<string> Semester { get; set; }
        public List<string> Term { get; set; }
        public List<string> Level { get; set; }
        public List<string> Grade { get; set; }
        public List<string> Subject { get; set; }
        public string ApprovalForm { get; set; }
        public string ApprovalType { get; set; }
        public string MultipleForm { get; set; }
    }

    public class Assign
    {
        public string Role { get; set; }
        public string User { get; set; }
    }

    public class History
    {
        public string UserIn { get; set; }
        public string DateIn { get; set; }
        public string Reason { get; set; }
        public string FieldChange { get; set; }
    }

    public class ReferenceDocument
    {
        public string Id { get; set; }
    }
}