using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class GetPmBenchmarkPeriodSettingsResult
    {
        public List<GetPmBenchmarkPeriodSettingResult_Pagination> AllPaginationData { get; set; }
        public List<string> AllIdByFilter { get; set; }
    }

    public class GetPmBenchmarkPeriodSettingResult_Pagination : ItemValueVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public CodeWithIdVm Period { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LastUpdated { get; set; }
        public bool IsDelete { get; set; }
    }
}
