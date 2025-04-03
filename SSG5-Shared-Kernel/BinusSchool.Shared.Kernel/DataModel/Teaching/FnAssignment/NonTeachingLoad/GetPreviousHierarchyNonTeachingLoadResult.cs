using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetPreviousHierarchyNonTeachingLoadResult
    {
        public List<GetRecentHierarchy> Hierarchies { get; set; } = new List<GetRecentHierarchy>();
    }

    public class GetRecentHierarchy
    {
        public string Name { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
