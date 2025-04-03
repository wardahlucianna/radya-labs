using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class MasterDataAttendanceDetailResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string AttendanceName { get; set; }
        public string ShortName { get; set; }
        public string AttendanceCategory { get; set; }
        public string AbsenceCategory { get; set; }
        public string ExcusedAbsenceCategory { get; set; }
        public string Status { get; set; }
        public bool IsNeedFileAttachment { get; set; }

    }
}
