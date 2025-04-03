using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade
{
    public class CopyNextAYMapStudentGradeRequest
    {
        public CopyNextAYMapStudentGradeRequest()
        {
            ExcludeStudentIds = new List<string>();
        }
        public string IdAcademicYearTarget { get; set; }
        public string IdAcademicYearSource { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public IEnumerable<string> ExcludeStudentIds { get; set; }
    }
}
