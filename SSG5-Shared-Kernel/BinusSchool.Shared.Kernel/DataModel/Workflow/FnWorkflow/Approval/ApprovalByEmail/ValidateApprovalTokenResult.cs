using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail
{
    public class ValidateApprovalTokenResult
    {
        public bool IsValid { get; set; }
        public string IdApprovalToken { get; set; }
        public string IdTransaction { get; set; }
        public ApprovalModule? Module { get; set; }
        public ApprovalStatus? Action { get; set; }
        public bool ShowAsError { get; set; }
        public string ReturnMessage { get; set; }
    }
}
