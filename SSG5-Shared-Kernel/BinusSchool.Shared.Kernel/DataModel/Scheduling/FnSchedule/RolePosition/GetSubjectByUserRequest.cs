using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetSubjectByUserRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public List<string> ListIdTeacherPositions { get; set; }
        public bool? IsAttendance { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsClassDiary { get; set; }
        public bool? IsLessonPlan { get; set; }
    }
}
