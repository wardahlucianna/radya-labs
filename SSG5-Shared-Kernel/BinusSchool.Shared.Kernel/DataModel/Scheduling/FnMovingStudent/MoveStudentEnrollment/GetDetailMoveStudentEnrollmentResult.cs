using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetDetailMoveStudentEnrollmentResult
    {
        public string IdHomeroomStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string Homeroom { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroom { get; set; }
        public string IdGrade { get; set; }
        public bool IsShowSubjectLevel { get; set; }
        public List<GetMovingStudentEnrollment> MovingStudentEnrollment { get; set; }
    }

    public class GetMovingStudentEnrollment
    {
        public DateTime? RegisteredDate { get; set; }
        public string idhomeroomStudentEnrollment { get; set; }
        public string idLesson { get; set; }
        public string idSubject { get; set; }
        public string idSubjectLevel { get; set; }
        public string subjectName { get; set; }
        public List<ItemValueVm> subjectLevel { get; set; }
    }


}
