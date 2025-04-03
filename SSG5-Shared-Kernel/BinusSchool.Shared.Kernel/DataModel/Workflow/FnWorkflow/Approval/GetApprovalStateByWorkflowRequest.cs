using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetApprovalStateByWorkflowRequest
    {
        public string IdApprovalWorkflow { get; set; }
        public string StateType { get; set; }
    }
}
