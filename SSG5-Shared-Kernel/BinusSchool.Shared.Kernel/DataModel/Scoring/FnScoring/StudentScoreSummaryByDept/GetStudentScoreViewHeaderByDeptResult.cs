using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByDept
{
    public class GetStudentScoreViewHeaderByDeptResult
    {
        public List<SubComponentCounterVm> SubComponentCounterData { set; get; }
    }
    public class SubComponentCounterVm
    {
        public string IdSubComponentCounter { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }

        public ComponentVm ComponentData { set; get; }
        public SubComponentVm SubComponentData { set; get; }

    }
    public class ComponentVm
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }

    public class SubComponentVm
    {
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }
}
