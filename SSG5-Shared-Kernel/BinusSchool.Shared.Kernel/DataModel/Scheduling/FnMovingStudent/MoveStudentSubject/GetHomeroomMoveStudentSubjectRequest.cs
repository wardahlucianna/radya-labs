using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetHomeroomMoveStudentSubjectRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester {  get; set; }
        public string IdGradeOld { get; set; }
    }
}
