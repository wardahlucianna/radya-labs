using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.CounterCategorySettings
{
    public class AddCounterCategoryRequest
    {
        public string ShortName { get; set; }
        public string Name { get; set; }
        public bool CurentStatus { get; set; }
        public string IdSchool { get; set; }
        public string IdCounterCategory { get; set; }

    }
}
