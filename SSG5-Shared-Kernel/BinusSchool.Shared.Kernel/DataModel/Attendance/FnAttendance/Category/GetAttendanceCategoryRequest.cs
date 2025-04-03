using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Category
{
    public class GetAttendanceCategoryRequest
    {
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public AttendanceCategory? AttendanceCategory { get; set; }
    }
}
