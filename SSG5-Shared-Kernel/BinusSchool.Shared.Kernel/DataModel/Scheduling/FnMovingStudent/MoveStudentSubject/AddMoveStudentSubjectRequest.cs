using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class AddMoveStudentSubjectRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester {  get; set; }
        //public string IdHomeroomOld { get; set; }
        public string IdLessonOld { get; set; }
        public string IdSubjectOld { get; set; }
        public string IdSubjectLevelOld { get; set; }
        //public string IdHomeroom { get; set; }
        public string IdLesson { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsDelete { get; set; }
        public bool IsSendEmail { get; set; }
        public string Note { get; set; }
        public List<string> IdHomeroomStudents { get; set; }
    }
}
