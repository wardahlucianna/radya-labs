using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.HierarchyMapping
{
    public class HierarchyMappingDetailResult : HierarchyMappingResult
    {
        public List<HierarchyResult> Parents { get; set; }
    }

    public class HierarchyResult
    {
        public string IdRolePosition { get; set; }
        public CodeWithIdVm Role { get; set; }
        public CodeWithIdVm TeacherPosition { get; set; }
        public List<HierarchyResult> Childs { get; set; }
    }
}
