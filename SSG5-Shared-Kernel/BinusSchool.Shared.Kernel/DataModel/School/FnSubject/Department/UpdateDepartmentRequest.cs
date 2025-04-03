using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSubject.Department
{
    public class UpdateDepartmentRequest
    {
        public string Id { get; set; }
        public string IdAcadyear { get; set; }
        public string DepartmentName { get; set; }
        public int LevelType { get; set; }
        public List<string> IdLevel { get; set; }
        public string Level { get; set; }
    }

}
