using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentCopyByGradeRequest : CollectionRequest
    {
        public string IdAcademicYearTarget { get; set; }
        public string IdAcademicYearSource { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; } 
    }
}
