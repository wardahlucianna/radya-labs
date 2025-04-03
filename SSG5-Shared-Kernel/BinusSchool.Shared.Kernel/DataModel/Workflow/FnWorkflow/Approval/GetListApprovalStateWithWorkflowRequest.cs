using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetListApprovalStateWithWorkflowRequest
    {
        public string IdWorkflow { get; set; }
        public string WithoutState { get; set; }
    }
}
