using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class AttendanceNameBySystem
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
        public AttendanceStatus Status { get; set; }
        public bool IsNeedFileAttachment { get; set; }
    }
}
