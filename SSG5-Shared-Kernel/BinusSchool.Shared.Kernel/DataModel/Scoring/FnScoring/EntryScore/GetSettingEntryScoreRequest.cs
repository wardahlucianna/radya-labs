using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class GetSettingEntryScoreRequest
    {
        public string IdSchool { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
    }
}
