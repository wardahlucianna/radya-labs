using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail
{
    public class DeactivateApprovalTokenRequest
    {
        public string IdApprovalToken { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
    }
}
