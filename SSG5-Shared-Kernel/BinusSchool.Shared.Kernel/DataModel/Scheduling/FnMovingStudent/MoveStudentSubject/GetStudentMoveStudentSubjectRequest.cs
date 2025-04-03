using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetStudentMoveStudentSubjectRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLessonOld { get; set; }
    }
}
