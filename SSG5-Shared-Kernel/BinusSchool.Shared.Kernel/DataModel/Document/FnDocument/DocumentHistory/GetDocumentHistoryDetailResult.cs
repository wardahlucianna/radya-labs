using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentHistory
{
    public class GetDocumentHistoryDetailResult
    {
        public string Id { get; set; }
        public string UserNameChangeBy { get; set; }
        public string JsonFormElementNew { get; set; }
        public string JsonFormElementOld { get; set; }
        public string JsonDocumentValueOld { get; set; }
        public string JsonDocumentValueNew { get; set; }
        public string TypeChange { get; set; }
        public string AcademicYears { get; set; }
        public string Subject { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Semester { get; set; }
        public string Term { get; set; }
        public string Comment { get; set; }
        public RevisionComment Revision { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public IEnumerable<NoteFiledChange> NoteFiledChange { get; set; }
        public IEnumerable<HistoryApproval> HistoryApproval { get; set; }
    }

    public class RevisionComment
    {
        public string Comment { get; set; }
        public string UserAction { get; set; }
        public DateTime? DateAction { get; set; }
    }

    public class NoteFiledChange 
    {
        public string FieldName { get; set; }
        public string Note { get; set; }
    }

    public class HistoryApproval
    {
        public string Name { get; set; }
        public string RoleAction { get; set; }
        public DateTime? DateAction { get; set; }
        public string Action { get; set; }
        
    }
}
