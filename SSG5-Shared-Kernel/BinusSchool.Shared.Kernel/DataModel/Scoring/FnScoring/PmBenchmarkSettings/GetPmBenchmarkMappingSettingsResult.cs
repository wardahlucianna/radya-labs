using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class GetPmBenchmarkMappingSettingsResult
    {
        public List<GetPmBenchmarkMappingSettingsResult_Pagination> AllPaginationData { get; set; }
        public List<string> AllIdByFilter { get; set; }
    }

    public class GetPmBenchmarkMappingSettingsResult_Pagination : ItemValueVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public CodeWithIdVm Period { get; set; }
        public string ComponentName { get; set; }
        public string OrderNumber { get; set; }
        public ItemValueVm ScoreOption { get; set; }
        public string LastUpdated { get; set; }
        public bool IsDelete { get; set; }
    }
}
