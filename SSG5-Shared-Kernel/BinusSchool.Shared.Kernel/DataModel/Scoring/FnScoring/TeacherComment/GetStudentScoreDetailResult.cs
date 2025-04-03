using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetStudentScoreDetailResult
    {
        //public string Subject { get; set; }
        //public string Score { get; set; }
        public string IdSubjectType { get; set; }
        public string SubjectType { get; set; }
        public List<GetListScoreDetail> ListScore { get; set; }
    }
}
