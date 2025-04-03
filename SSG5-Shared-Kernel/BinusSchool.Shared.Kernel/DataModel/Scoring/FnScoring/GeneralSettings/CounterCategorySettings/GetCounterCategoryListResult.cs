using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.CounterCategorySettings
{
    public class GetCounterCategoryListResult
    {
        public string ShortName { get; set; }
        public string Name { get; set; }
        public bool CurrentStatus { get; set; }
        public string IdCounterCategory { get; set; }
        public bool IsDelete { get; set; }
    }
}
