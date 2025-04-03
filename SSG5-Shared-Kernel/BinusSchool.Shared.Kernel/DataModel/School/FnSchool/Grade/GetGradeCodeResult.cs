using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeCodeResult : CodeWithIdVm
    {
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm School { get; set; }
    }
}
