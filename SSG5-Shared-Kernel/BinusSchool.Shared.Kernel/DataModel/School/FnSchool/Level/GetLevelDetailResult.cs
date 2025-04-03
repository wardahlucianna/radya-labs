using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Level
{
    public class GetLevelDetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public int OrderNumber { get; set; }
    }
}
