using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.School.FnSubject.Department
{
    public class GetDepartmentResult : IItemValueVm
    {
        public string Id { get; set; }
        public CodeWithIdVm Acadyear { get; set; }
        public string Level { get; set; }
        public string Description { get; set; }
        public IEnumerable<CodeWithIdVm> Subjects{get;set;}
        public string IdLevel { get; set; }
    }
}
