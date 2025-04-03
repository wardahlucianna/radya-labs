using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom
{
    public class GetHomeroomNewMoveStudentHomeroomRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGradeOld { get; set; }
        public string IdHomeroomOld { get; set; }
    }
}
