using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreComponent
{
    public class MasterSubComponentCounterRequest
    {
        public string IdSubComponent { set; get; }
        public List<string> TeacherPositionList { set; get; }
        public string idLesson { set; get; }
    
    }
}
