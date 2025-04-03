using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class GetHomeroomTeacherPrivilegeRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public int? Semester { get; set; }
        public bool IncludeClassAdvisor { get; set; }
        public bool IncludeSubjectTeacher { get; set; }
    }
}
