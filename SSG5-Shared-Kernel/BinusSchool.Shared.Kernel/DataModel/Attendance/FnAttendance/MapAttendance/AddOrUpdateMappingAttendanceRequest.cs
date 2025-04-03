using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance
{
    public class AddOrUpdateMappingAttendanceRequest
    {
        public string Id { get; set; }
        public string IdLevel { get; set; }
        public List<string> AttendanceName { get; set; }
        public AbsentTerm AbsentTerms { get; set; }
        public bool IsNeedValidation { get; set; }
        public bool IsUseWorkHabit { get; set; }
        public bool IsDueToLateness { get; set; }
        public bool UsingCheckboxAttendance { get; set; }
        public bool ShowingModalReminderAttendanceEntry { get; set; }
        public RenderAttendance RenderAttendance { get; set; }
        public List<AbsentBy> AbsentMapping { get; set; }
        public List<string> MappingWorkhabit { get; set; }
    }

    public class AbsentBy
    {
        public string IdTeacherPosition { get; set; }
        public List<ListMappingAttendance> ListMappingAttendance { get; set; }
    }
    
    public class ListMappingAttendance
    {
        public bool IsNeedValidation { get; set;}
        public string IdAttendance { get; set; }
    }
}
