using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetListApprovalTypeScoringRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdUser { set; get; }
    }
}
