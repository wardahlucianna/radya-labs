using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class AddMasterDataAttendanceRequest
    {
        public string IdAcademicYear { get; set; }
        public string AttendanceName { get; set; }
        public string ShortName { get; set; }
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
        public AttendanceStatus Status { get; set; }
        public bool IsNeedFileAttachment { get; set; }

    }
}
