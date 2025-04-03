using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreViewSubjectHeaderByHomeroomResult
    {
        public List<ScoreViewSubjectHeaderSubjectLevelVm> SubjectLevelStudentScoreList { set; get; }
    }

    public class ScoreViewSubjectHeaderSubjectLevelVm
    {
        public string SubjectLevel { set; get; }
        public List<ScoreViewSubjectHeader_ComponentVm> ComponentList { set; get; }

    }

  


 }
