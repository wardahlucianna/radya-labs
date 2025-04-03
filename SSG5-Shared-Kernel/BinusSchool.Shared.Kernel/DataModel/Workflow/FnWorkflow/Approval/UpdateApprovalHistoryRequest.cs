using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval 
{ 
    public class UpdateApprovalHistoryRequest : AddApprovalHistoryRequest
    {
        public string Id { get; set; }
    }
}
