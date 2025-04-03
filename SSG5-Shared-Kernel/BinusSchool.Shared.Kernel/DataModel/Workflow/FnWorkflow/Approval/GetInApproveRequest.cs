using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetInApproveRequest
    {
        public string IdFromState { get; set; }
        public int Action { get; set; }
    }
}
