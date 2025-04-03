using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAffectiveScore
{
    public class GetScoreViewStudentAffectiveScoreDetailResult
    {
        public string Summary { set; get; }
        public List<GetScoreViewStudentAffectiveScoreDetail_SubComponentVm> DetailList { set; get; }
    }

    public class GetScoreViewStudentAffectiveScoreDetail_SubComponentVm
    {
        public string SubjectName { set; get; }
        public string SubComponentName { set; get; }
        public string Score { set; get; }
    }
}
