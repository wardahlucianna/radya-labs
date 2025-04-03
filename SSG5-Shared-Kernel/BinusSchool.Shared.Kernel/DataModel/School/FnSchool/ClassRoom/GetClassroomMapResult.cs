using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoom
{
    public class GetClassroomMapResult : CodeWithIdVm
    {
        public IEnumerable<CodeWithIdVm> Pathways { get; set; }
    }
}
