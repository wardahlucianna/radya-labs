using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition
{
    public class GetTeacherPositionHasNotNonTeachingLoadRequest : CollectionSchoolRequest
    {
        public string PositionCode { get; set; }
        public string IdAcadyear { get; set; }
        public AcademicType Category { get; set; }
    }
}
