using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetListFilterLevelGradeByTermResult : CodeWithIdVm
    {
        public int OrderNumber { set; get; }
        public List<GetListFilterLevelGradeResult_Grade> Grades { set; get; }
    }

    public class GetListFilterLevelGradeResult_Grade : CodeWithIdVm
    {
        public int OrderNumber { set; get; }
    }
}
