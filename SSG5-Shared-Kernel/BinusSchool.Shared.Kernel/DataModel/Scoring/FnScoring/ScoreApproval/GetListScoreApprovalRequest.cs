using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetListScoreApprovalRequest : CollectionSchoolRequest
    {
        public string IdTeacherPosition { set; get; }
        public string IdAcademicYear { set; get; }
        public int? ApprovalType { set; get; }
        public int? ApprovalStatus { set; get; }
    }
}
