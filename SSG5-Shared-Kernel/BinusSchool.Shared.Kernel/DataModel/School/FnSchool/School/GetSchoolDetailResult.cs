using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.School
{
    public class GetSchoolDetailResult: ItemValueVm
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public AuditableResult Audit { get; set; }
    }
}
