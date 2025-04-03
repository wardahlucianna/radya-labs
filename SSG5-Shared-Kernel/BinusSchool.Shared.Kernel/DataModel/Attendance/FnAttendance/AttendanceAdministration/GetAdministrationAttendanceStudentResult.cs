using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAdministrationAttendanceStudentResult
    {
        public string IdStudent { get; set; }
        public string Name { get; set; }
        public CodeWithIdVm HomeroomName { get; set; }
        public string IdGrade { get; set; }
    }
}
