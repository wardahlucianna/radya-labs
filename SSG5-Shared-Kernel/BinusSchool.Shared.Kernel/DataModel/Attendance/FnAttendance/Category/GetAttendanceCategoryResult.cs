using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Category
{
    public class GetAttendanceCategoryResult
    {
        public AttendanceCategory AttendanceCategory { get; set; }
        public IEnumerable<AttendanceCategoryAttendance> PresentAttendances { get; set; }
        public IDictionary<ExcusedAbsenceCategory, IEnumerable<AttendanceCategoryAttendance>> ExcusedAttendance { get; set; }
        public IDictionary<AbsenceCategory, IEnumerable<AttendanceCategoryAttendance>> UnexcusedAttendance { get; set; }
    }

    public class AttendanceCategoryAttendance : CodeWithIdVm
    {
        public bool NeedAttachment { get; set; }
    }
}
