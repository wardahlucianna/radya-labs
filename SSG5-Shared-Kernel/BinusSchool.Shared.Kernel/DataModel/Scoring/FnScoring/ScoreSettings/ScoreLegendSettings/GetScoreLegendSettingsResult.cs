using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings
{
    public class GetScoreLegendSettingsResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public bool IsUsed { get; set; }
        public List<string> LegendList { get; set; }
    }
}
