using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetApprovalHistoryByUserResult
    {
        public string IdForUser { get; set; }
        public string IdFromState { get; set; }
        public string IdApprovalTask { get; set; }
        public string IdDocument { get; set; }
        public string Action { get; set; }
    }
}
