using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreComponent
{
    public class MasterSubComponentCounterResult
    {
        public MasterSubComponentCounterResult()
        {
            SubComponentCounterList = new List<SubComponentCounterDataVm>();
        }
        public bool CanAddCounter { set; get; }
        public bool EnableTeacherJudgement { set; get; }
        public List<SubComponentCounterDataVm> SubComponentCounterList { set; get; }
        public SubComponentData SubComponent { set; get; }
    }

    public class SubComponentCounterDataVm
    {

        public string IdSubComponentCounter { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
    }

    public class SubComponentData
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
    }
}
