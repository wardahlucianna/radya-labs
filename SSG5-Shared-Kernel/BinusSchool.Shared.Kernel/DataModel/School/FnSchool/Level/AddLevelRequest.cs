using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Level
{
    public class AddLevelRequest : CodeVm
    {
        public string IdAcadyear { get; set; }
        public int OrderNumber { get; set; }
    }
}
