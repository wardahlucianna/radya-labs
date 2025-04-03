using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSubject.Department
{
    public class GetDepartmentRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
    }
}
