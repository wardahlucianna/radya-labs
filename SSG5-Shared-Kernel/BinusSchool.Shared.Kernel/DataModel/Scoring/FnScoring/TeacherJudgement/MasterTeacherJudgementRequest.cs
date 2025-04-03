using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class MasterTeacherJudgementRequest
    {
        public string IdStudent { set; get; }
        public SubComponentData SubComponentDataVm { set; get; }
        public ComponentData ComponentDataVM { set; get; }
        public SubjectData SubjectDataVM { set; get; }
    }

    public class SubComponentData
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
    }

    public class ComponentData
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }

    public class SubjectData
    {
        public string IdSubjectScoreSetting { set; get; }
    }
}
