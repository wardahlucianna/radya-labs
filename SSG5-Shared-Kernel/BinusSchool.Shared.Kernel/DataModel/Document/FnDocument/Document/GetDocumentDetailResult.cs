using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.Document
{
    public class GetDocumentDetailResult
    {
        public string Id { get; set; }
        public string IdFormBuilderTemplate { get; set; }
        public string JsonDocumentValue { get; set; }
        public string JsonFormElement { get; set; }
        public object Status { get; set; }
        public string AcademicYears { get; set; }
        public string Subject { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Semester { get; set; }
        public string Term { get; set; }
        public string ForNextRole { get; set; }
        public string CreateBy { get; set; }
        public RevisionComment Revision { get; set; }
        public DateTime? CreateDate { get; set; }
        public IEnumerable<HistoryChange> HistroyCahnges { get; set; }
        public IEnumerable<HistoryApproval> HistoryApprovals { get; set; }
        public ApprovalTask ApprovalTask { get; set; }
    }

    public class RevisionComment 
    {
        public string Comment { get; set; }
        public string UserAction { get; set; }
        public DateTime? DateAction { get; set; }
    }

    public class HistoryChange
    {
        public string UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public string ValueBeforeUpdate { get; set; }
        public string ValueAfterUpdate { get; set; }
        public string FieldChange { get; set; }
    }

    public class HistoryApproval 
    {
        public string Name { get; set; }
        public string RoleAction { get; set; }
        public DateTime? DateAction { get; set; }
        public string Action { get; set; }
        public string NextRoleAction { get; set; }
    }

    public class ApprovalTask 
    {
        public string IdForUser { get; set; }
        public string IdFromState { get; set; }
        public string IdApprovalTask { get; set; }
        public string IdDocument { get; set; }
        public string Action { get; set; }
    }
}
