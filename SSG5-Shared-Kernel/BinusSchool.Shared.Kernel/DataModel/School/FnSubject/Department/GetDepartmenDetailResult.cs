using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSubject.Department
{
    public class GetDepartmenDetailResult
    {
        public string Id { get; set; }
        public CodeWithIdVm Acadyear { get; set; }
        public int TypeLevel { get; set; }
        public string Level { get; set; }
        public string DepartmentName { get; set; }
        public List<string> IdLevel { get; set; }      
        public string TypeLevelName { get; set; }
    }
}
