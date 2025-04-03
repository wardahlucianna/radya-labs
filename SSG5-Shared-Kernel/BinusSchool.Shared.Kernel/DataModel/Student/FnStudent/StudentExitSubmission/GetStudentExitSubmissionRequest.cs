
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission
{
    public class GetStudentExitSubmissionRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string Status { get; set; }
    }
}
