using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentByGradeRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public bool? IncludePathway { get; set; }
        public IEnumerable<string> ExceptIds { get; set; }
    }
}