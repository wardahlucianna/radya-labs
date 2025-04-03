using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetApprovalStateByWorkflowResult
    {
        public string Id { get; set; }
        public string IdApprovalWorkflow { get; set; }
        public string IdRole { get; set; }
        public string StateName { get; set; }
        public string StateType { get; set; }
        public int StateNumber { get; set; }
    }
}
