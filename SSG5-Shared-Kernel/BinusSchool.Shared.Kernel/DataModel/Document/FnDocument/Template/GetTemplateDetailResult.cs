using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Template
{
    public class GetTemplateDetailResult
    {
        public string Id { get; set; }
        public CodeWithIdVm SchoolDocumentCategory { get; set; }
        public string Acadyear { get; set; }
        public string Semester { get; set; }
        public string Term { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public ApprovalVm Approval { get; set; }
        public bool IsApprovalForm { get; set; }
        public bool IsMultipleForm { get; set; }
        public string JsonFormElement { get; set; }
        public string JsonSchema { get; set; }
        public string IdFormTemplateRefence { get; set; }
        public string IdSchool { get; set; }
        public AssignmentRoleResult FormBuilderAssignmentRole { get; set; }
    }

    public class AssignmentRoleResult : ItemValueVm
    {
        public IEnumerable<ItemValueVm> FormBuilderAssignmentUsers { get; set; }
    }
}
