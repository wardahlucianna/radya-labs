using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class UnresolvedAttendanceGroupV2Result 
    {
        public DateTime Date { get; set; }
        public string ClassID { get; set; }
        public AbsentTerm TermAbsent { get; set; }
        public string IdLesson { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Session { get; set; }
        public int TotalStudent { get; set; }
    }

    public class ScheduleAttendanceResult
    {
        public string IdScheduleLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdHomeroom { get; set; }
    }

    public class ScheduleLessonResult
    {
        public string Id { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdLesson { get; set; }
        public string ClassID { get; set; }
        public string idSession { get; set; }
        public string nameSession { get; set; }
        public string IdGrade { get; set; }
        public string IdLevel { get; set; }
        public string SessionID { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IdDay { get; set; }
        public string IdSchool { get; set; }
        public int Semester { get; set; }
    }
}
