using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping
{
    public class UpdateScoreViewMappingStatusRequest
    {
        public string IdReportScoreTableTemplate { set; get; }
        public bool CurrentStatus { set; get; }

    }
}
