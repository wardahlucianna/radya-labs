using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetSubjectMoveStudentSubjectRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string IdHomeroomOld { get; set; }
        public string IdLessonOld { get; set; }
    }
}
