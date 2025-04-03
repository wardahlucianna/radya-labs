using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetMoveStudentEnrollmentRequest : CollectionSchoolRequest
    {
        public string idAcademicYear { get; set; }
        public int semester { get; set; }
        public string idLevel { get; set; }
        public string idGrade { get; set; }
        public string idHomeroom { get; set; }
        public string studentName { get; set; }
    }
}
