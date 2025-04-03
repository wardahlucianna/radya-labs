using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ComponentDescription
{
    public class GetComponentDescriptionResult : CodeWithIdVm
    {
        public string Level { get; set; }
        public string Grade { get; set; }
        public string SubjectType { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Term { get; set; }
        public string LastUpdated { get; set; }
    }
}
