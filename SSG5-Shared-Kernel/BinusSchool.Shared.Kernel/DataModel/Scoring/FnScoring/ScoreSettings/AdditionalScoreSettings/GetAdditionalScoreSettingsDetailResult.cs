using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings
{
    public class GetAdditionalScoreSettingsDetailResult
    {
        
        public string IdScoreAdditionalCode { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        public CodeWithIdVm Grade { set; get; }
        public string Category { set; get; }
        public decimal Key { set; get; }
        public bool ShowScoreAsNA { set; get; }
        public bool CurrentStatus { set; get; }
        public string LongDesc { set; get; }
    }
}
