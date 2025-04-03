using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetScoreComponentDefaultSettingDetailResult
    {
        public List<GetScoreComponentDefaultSettingDetailResult_Setting> DefaultSettings { get; set; }
        public List<GetScoreComponentDefaultSettingDetailResult_Term> Terms { get; set; }
    }

    public class GetScoreComponentDefaultSettingDetailResult_Setting
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string InputType { get; set; }
        public string Value { get; set; }
        public bool ShowInScoreSetting { get; set; }
        public int? OrderNo { get; set; }
    }

    public class GetScoreComponentDefaultSettingDetailResult_Term
    {
        public ItemValueVm Period { get; set; }
        public bool IsScorePresent { get; set; }
        public bool IsChecked { get; set; }
        public ItemValueVm ScoreLegend { get; set; }
        public decimal? Weight { get; set; }
    }
}
