using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectScoreGroup
{
    public class GetSubjectScoreGroupListResult : ItemValueVm
    {
        public int OrderNoSubjectGroup { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        public List<CodeWithIdVm> Levels { set; get; }
        public List<CodeWithIdVm> Grades { set; get; }
        public ItemValueVm SubjectGroup { set; get; }
        public List<CodeWithIdVm> Subject { set; get; }
    }
}
