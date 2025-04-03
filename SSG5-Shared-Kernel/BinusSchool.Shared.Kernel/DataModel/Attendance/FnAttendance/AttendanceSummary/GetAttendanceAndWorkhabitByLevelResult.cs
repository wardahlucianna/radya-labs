using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAttendanceAndWorkhabitByLevelResult
    {
        public ICollection<CodeWithIdVm> Attendances { get; set; }
        public ICollection<CodeWithIdVm> Workhabits { get; set; }
    }


}
