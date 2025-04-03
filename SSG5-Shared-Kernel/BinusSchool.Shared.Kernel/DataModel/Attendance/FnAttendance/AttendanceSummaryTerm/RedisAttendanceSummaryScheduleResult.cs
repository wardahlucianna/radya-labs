using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryScheduleResult
    {
        public string IdLesson { get; set; }
        public RedisAttendanceSummaryTeacher Teacher { get; set; }
        public string IdWeek { get; set; }
        public string IdDay { get; set; }
        public string IdSession { get; set; }

    }

    public class RedisAttendanceSummaryTeacher
    {
        public string IdUser { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
