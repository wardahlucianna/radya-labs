using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryHomeroomTeacherResult : CodeWithIdVm
    {
        public RedisAttendanceSummaryTeacher Teacher { get; set; }
        public string IdHomeroom { get; set; }
        public string IdGrade { get; set; }
        public string IdClassroom { get; set; }
        public CodeWithIdVm Position { get; set; }
        public bool IsAttendance { get; set; }
    }
}
