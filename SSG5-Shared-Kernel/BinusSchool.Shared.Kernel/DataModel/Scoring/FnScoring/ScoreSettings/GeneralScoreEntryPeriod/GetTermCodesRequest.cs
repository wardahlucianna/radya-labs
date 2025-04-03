using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.GeneralScoreEntryPeriod
{
    public class GetTermCodesRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
    }
}
