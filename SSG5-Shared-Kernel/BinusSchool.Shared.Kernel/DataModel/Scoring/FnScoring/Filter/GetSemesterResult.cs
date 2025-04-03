using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetSemesterResult
    {
        public ItemValueVm Semester { set; get; }
        public List<ItemValueVm> Term { set; get; }
    }
}
