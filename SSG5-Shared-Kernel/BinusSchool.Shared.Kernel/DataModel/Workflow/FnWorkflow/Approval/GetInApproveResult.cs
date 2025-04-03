using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval
{
    public class GetInApproveResult
    {
        public string ToStateType { get; set; }
        public string FromStateType { get; set; }
        public string FromStateIdRoleAction { get; set; }
        public string IdFromState { get; set; }
        public string ToStateIdRoleAction { get; set; }
        public string IdToState { get; set; }
        public ApprovalStatus Status { get; set; }
    }
}
