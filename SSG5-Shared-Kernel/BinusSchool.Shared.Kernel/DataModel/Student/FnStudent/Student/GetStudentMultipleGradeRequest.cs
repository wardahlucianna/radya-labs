using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentMultipleGradeRequest : CollectionRequest
    {
        public string IdAcadYear { get; set; }
        public IEnumerable<string> IdGrades { get; set; }
    }
}
