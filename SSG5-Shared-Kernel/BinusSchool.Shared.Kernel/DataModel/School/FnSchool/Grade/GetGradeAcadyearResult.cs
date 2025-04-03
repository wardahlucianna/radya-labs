using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeAcadyearResult : CodeWithIdVm
    {
        public int OrderNumber { get; set; }
        public IEnumerable<CodeWithIdVm> Grades { get; set; }
    }
}
