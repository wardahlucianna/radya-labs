using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.MoveStudentPathway
{
    public class AddMoveStudentPathwayRequest
    {
        public IEnumerable<string> IdStudents { get; set; }
        public string IdPathway { get; set; }
    }
}