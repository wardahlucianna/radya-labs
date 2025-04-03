using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class GetListFilterPmBenchmarkMappingSettingsResult
    {
        public List<GetListFilterPmBenchmarkMappingSettingsResult_Level> Level { get; set; }
        public List<GetListFilterPmBenchmarkMappingSettingsResult_Grade> Grade { get; set; }
        public List<GetListFilterPmBenchmarkMappingSettingsResult_Term> Period { get; set; }
        public List<GetListFilterPmBenchmarkMappingSettingsResult_Component> Component {get; set; }
    }

    public class GetListFilterPmBenchmarkMappingSettingsResult_Level : ItemValueVm
    {

    }

    public class GetListFilterPmBenchmarkMappingSettingsResult_Grade : ItemValueVm
    {
        public string IdLevel { get; set; }

    }

    public class GetListFilterPmBenchmarkMappingSettingsResult_Term : CodeVm
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
    
    public class GetListFilterPmBenchmarkMappingSettingsResult_Component
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string PeriodCode { get; set; }
        public string ComponentName { get; set; }
    }
}
