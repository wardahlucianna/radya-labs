using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.TeacherComment
{
    public class GetApprovalWorkflowRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
    }
}
