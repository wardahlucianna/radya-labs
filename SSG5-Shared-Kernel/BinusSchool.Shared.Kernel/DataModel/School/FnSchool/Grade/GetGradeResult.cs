using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeResult : CodeWithIdVm
    {
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm School { get; set; }
        public int OrderNumber { get; set; }
    }
}
