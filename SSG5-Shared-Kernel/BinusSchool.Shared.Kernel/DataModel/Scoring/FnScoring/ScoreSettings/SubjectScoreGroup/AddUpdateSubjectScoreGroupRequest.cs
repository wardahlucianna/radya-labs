using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectScoreGroup
{
    public class AddUpdateSubjectScoreGroupRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdSubjectGroup { set; get; }
        public string SubjectGroupName { set; get; }
        public int OrderNoSubjectGroup { set; get; }
        public List<string> Subjects { set; get; }
    }
}
