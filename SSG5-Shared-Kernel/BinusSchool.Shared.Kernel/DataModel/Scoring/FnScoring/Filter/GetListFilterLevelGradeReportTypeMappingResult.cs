using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetListFilterLevelGradeReportTypeMappingResult
    {
        public CodeWithIdVm Level { set; get; }
        public List<CodeWithIdVm> Grade { set; get; }
    }
}
