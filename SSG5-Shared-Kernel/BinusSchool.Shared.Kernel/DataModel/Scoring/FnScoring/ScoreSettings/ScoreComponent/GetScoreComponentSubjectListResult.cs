using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetScoreComponentSubjectListResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm SubjectType { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm SubjectLevel { get; set; }
        public ItemValueVm SubjectWithSubjectLevel { get; set; }
        public List<GetScoreComponentSubjectListResult_Component> SubjectComponents { get; set; }
        public bool CanDelete { get; set; }
    }

    public class GetScoreComponentSubjectListResult_Component
    {
        public int Semester { get; set; }
        public ItemValueVm Period { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public int Counter { get; set; }
        public decimal Weight { get; set; }
    }
}
