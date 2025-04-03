using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetNonTeachLoadResult : ItemValueVm
    {
        public string Acadyear { get; set; }
        public AcademicType Category { get; set; }
        public string Position { get; set; }
        public int Load { get; set; }
    }
}
