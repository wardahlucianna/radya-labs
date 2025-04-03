using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class PostAttendanceAdministrationRequest
    {
        public bool IsAllDay {get;set;}
        public string IdUser {get;set; }
        public string IdSchool {get;set; }
        public IEnumerable<PostAttendanceAdministrationStudentRequest> Students { get; set; }
    }

    public class PostAttendanceAdministrationStudentRequest
    {
        public string IdStudent { get; set; }
        public string HomeRoom { get; set; }
        public string IdAttendance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartPeriod { get; set; }
        public TimeSpan EndPeriod { get; set; }
        public string Reason { get; set; }
        public string AbsencesFile { get; set; }
        public bool IncludeElective { get; set; }
        public bool NeedValidation { get; set; }
        public string IdGrade { get; set; }
        public bool SendEmail { get; set; }
    }
}
