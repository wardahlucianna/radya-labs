using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings
{
    public class GetScoreLegendSettingsDetailResult
    {
        public GetScoreLegendSettingsDetailResult()
        {
            ScoreLegendDetails = new List<GetScoreLegendDetailVm>();
        }
        public string IdScoreLegend { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get;set; }  
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public List<GetScoreLegendDetailVm> ScoreLegendDetails { get; set; }    
    }
    public class GetScoreLegendDetailVm
    {
        public string IdScoreLegendDetail { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string Grade { get; set; }
        public string Description { get; set; }
        public string ConvertScore { get; set; }
    }
}
