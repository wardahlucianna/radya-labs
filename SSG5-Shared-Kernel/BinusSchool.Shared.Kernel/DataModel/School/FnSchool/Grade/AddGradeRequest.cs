using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class AddGradeRequest : CodeVm
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
    }
}
