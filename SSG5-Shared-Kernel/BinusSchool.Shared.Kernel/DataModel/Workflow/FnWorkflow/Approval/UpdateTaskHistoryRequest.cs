using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class UpdateTaskHistoryRequest
    {
        public string Id { get; set; }
        public ApprovalStatus Action { get; set; }
        public string UserID { get; set; }
    }
}
