using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetScoreOptionRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdScoreOption { get; set; }
    }
}
