using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetApprovalHistoryByUserRequest
    {
        public string IdUserAction { get; set; }
        public string IdDocument { get; set; }
        public ApprovalStatus Action { get; set; }
    }
}
