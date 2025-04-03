using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class EmailMoveStudentEnrollmentRequest
    {
        public string academicYear { get; set; }
        public int semester { get; set; }
        public string homeRoom { get; set; }
        public string studentId { get; set; }
        public string studentName { get; set; }
        public string newSubject { get; set; }
        public string newSubjectLevel { get; set; }
        public string oldSubject { get; set; }
        public string oldSubjectLevel { get; set; }
        public string effectiveDate { get; set; }
        public string note { get; set; }
        public List<string> IdUserTeacher { get; set; }
        public List<string> IdUserCc { get; set; }
    }
}
