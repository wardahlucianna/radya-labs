using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateAttendanceByMoveStudentEnrollRequest
    {
        public DateTime StartDate { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdHomeroomStudent { get; set; }
        public List<LessonMoveStudentEnroll> ListLessonMoveStudentEnroll { get; set; }
    }

    public class LessonMoveStudentEnroll
    {
        public string IdLessonOld { get; set; }
        public string IdLessonNew { get; set; }
    }
}
