using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetPreviousHierarchyNonTeachingLoadRequest
    {
        public string IdAcadyear { get; set; }
        public AcademicType Category { get; set; }
        public string IdPosition { get; set; }
    }
}
