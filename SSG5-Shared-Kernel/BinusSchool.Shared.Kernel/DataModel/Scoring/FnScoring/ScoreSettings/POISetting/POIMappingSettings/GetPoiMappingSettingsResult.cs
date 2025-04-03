using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings
{
    public class GetPoiMappingSettingsResult
    {
        public string Id { get; set; }
        public ItemValueVm Grade { get; set; }
        public int Semester { get; set; }
        public ItemValueVm UnitOfInquiry { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsCanDelete { get; set; }
    }
}
