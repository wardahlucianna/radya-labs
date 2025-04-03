using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class MappingAttendanceResult
    {
        public string Id { get; set; }
        public string IdLevel { get; set; }
        public bool IsNeedValidation { get; set; }
        public bool IsUseDueToLateness { get; set; }
        public bool IsUseWorkhabit { get; set; }
        public AbsentTerm AbsentTerms { get; set; }
    }
}
