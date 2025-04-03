using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class UpdateStudentEnrollmentRequest
    {
        public string IdHomeroom { get; set; }
        public IEnumerable<EnrollStudentToLesson> Enrolls { get; set; }
    }

    public class EnrollStudentToLesson
    {
        public string IdStudent { get; set; }
        public IEnumerable<LessonToEnroll> Lessons { get; set; }
    }

    public class LessonToEnroll
    {
        public string IdEnrollment { get; set; }
        public string IdLesson { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdSubject {get;set;}
    }
}
