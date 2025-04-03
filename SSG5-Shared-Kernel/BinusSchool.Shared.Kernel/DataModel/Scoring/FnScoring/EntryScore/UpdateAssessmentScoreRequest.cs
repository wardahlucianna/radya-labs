using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class UpdateAssessmentScoreRequest
    {
        public string IdApprovalWorkflow { get; set; }
        public bool IsUpdateAssessment { set; get; }
        public bool IsDeleteAssessment { set; get; }
        public UpdateAssessmentScoreRequest_AssessmentDetail Assessment { set; get; }
    }
    public class UpdateAssessmentScoreRequest_AssessmentDetail
    {
        public string IdCounter { set; get; }
        public DateTime DateCounter { set; get; }
        public string IdCounterCategory { set; get; }
        public string LongDesc { set; get; }
    }
}
