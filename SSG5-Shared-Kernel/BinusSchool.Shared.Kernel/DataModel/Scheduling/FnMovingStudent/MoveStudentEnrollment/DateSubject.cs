using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class DateSubject
    {
        public string idHomeroomStudentEnrollment { get; set; }
        public string idLesson { get; set; }
        public string idSubject { get; set; }
        public string idSubjectLevel { get; set; }
        public string subjectName { get; set; }
        public DateTime? dateIn { get; set; }
        public DateTime? registerDate { get; set; }
    }
}
