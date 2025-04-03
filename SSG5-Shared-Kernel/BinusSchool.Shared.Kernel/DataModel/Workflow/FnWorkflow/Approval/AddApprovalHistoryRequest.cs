using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class AddApprovalHistoryRequest
    {
        public string IdDocument { get; set; }
        public string IdFormState { get; set; }
        public string IdUserAction { get; set; }
        public ApprovalStatus Action { get; set; }
    }
}
