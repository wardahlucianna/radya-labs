using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class AddMoveStudentEnrollmentRequest
    {
        public string IdHomeroomStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public List<GetMoveStudentEnrollment> StudentEnrollment { get; set; }
    }

    public class GetMoveStudentEnrollment
    {
        public string IdHomeroomStudentEnrollment { get; set; }
        public string IdGrade { get; set; }
        public string IdLessonOld { get; set; }
        public string IdSubjectOld { get; set; }
        public string IdSubjectLevelOld { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IdLessonNew { get; set; }
        public string IdSubjectNew { get; set; }
        public string IdSubjectLevelNew { get; set; }
        public string Note { get; set; }
        public bool IsDelete { get; set; }
        public bool IsSendEmail { get; set; }
    }
}
