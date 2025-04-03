using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail
{
    public class CreateApprovalTokenRequest
    {
        public ApprovalModule Module { get; set; }
        public string IdTransaction { get; set; }
    }
}
