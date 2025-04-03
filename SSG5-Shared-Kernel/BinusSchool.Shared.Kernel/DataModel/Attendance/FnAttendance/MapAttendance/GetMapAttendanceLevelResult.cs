using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance
{
    public class GetMapAttendanceLevelResult : CodeWithIdVm
    {
        public CodeWithIdVm Acadyear { get; set; }
        public AbsentTerm Term { get; set; }
    }
}
