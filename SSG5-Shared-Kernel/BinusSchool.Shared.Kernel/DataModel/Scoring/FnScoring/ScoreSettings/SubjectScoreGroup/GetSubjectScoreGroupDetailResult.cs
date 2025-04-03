using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectScoreGroup
{
    public class GetSubjectScoreGroupDetailResult
    {
        public string IdSubjectGroup { set; get; }
        public string SubjectGroupDesc { set; get; }
        public int OrderNoSubjectGroup { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        
        public List<GetSubjectScoreGroupListResult_Subject> Subjects { set; get; }
    }

    public class GetSubjectScoreGroupListResult_Subject
    {
        public CodeWithIdVm Level { set; get; }
        public CodeWithIdVm Grade { set; get; }
        public CodeWithIdVm Subject { set; get; }
    }
}
