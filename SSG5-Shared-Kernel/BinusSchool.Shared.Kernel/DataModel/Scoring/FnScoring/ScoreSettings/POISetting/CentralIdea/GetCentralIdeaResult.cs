using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.CentralIdea
{
    public class GetCentralIdeaResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool CurrentStatus { get; set; }
        public bool IsEditable { get; set; }
        public bool IsDeletable { get; set; }
    }
}
