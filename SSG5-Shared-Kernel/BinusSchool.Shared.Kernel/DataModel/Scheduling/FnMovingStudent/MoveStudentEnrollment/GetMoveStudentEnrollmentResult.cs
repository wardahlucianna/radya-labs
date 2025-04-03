using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetMoveStudentEnrollmentResult : CodeWithIdVm
    {
        public string idHomeroomStudent { get; set; }
        public string studentName { get; set; }
        public string homeroom { get; set; }
    }

    public class listSubjectLesson
    {
        public string idHomeroomStudentEnrollment { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdLesson { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string subjectDescription { get; set; }
        public DateTime? registeredDate { get; set; }
    }
}
