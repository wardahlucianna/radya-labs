using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.GeneralScoreEntryPeriod
{
    public class GetAllSubjectsRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string Term { get; set; }
    }
}
