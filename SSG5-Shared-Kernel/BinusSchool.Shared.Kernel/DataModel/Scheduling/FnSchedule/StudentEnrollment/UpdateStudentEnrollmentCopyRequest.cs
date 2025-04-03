using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class UpdateStudentEnrollmentCopyRequest
    {
        public string IdAcadyearCopyTo { get; set; }
        public int SemesterCopyTo { get; set; }
        public string IdHomeroom { get; set; }
        public List<string> IdStudents { get; set; }
    }
}
