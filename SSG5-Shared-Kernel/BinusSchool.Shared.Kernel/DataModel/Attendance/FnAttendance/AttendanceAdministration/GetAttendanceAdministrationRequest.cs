using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAttendanceAdministrationRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
    }
}
