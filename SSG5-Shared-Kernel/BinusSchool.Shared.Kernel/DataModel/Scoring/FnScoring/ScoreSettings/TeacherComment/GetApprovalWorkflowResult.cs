using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.TeacherComment
{
    public class GetApprovalWorkflowResult
    {
        public ItemValueVm ApprovalWorkflow { get; set; }
        public List<string> ListApproval { get; set; }
    }
}
