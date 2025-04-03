using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnUser.HierarchyMapping
{
    public class AddHierarchyMappingRequest
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public List<HierarchyRequest> Hierarchies { get; set; }
    }
    public class HierarchyRequest
    {
        public string IdRolePosition { get; set; }
        public string IdRolePositionParent { get; set; }
    }
}
