using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreComponent
{
    public class MasterSubComponentResult
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
        public string Description { set; get; }
        public int OrderNo { set; get; }
    }
}
