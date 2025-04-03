using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade
{
    public class GetMapStudentGradeRequest : CollectionRequest
    {
        public string AcademicYear { get; set; }
        public string IdLevel { get; set; }
    }
}
