using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance
{
    public class GetMapAttendanceDetailResult : ItemValueVm
    {
        public NameValueVm School { get; set; }
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public IEnumerable<MapAttendanceDetailItem> Attendances { get; set; }
        public AbsentTerm Term { get; set; }
        public MapAttendanceTermSession TermSession { get; set; }
        public bool UseWorkhabit { get; set; }
        public bool NeedValidation { get; set; }
        public bool IsUseDueToLateness { get; set; }
        public IEnumerable<Workhabits> Workhabits { get; set; }
        public bool UsingCheckboxAttendance { get; set; }
        public RenderAttendance RenderAttendance { get; set; }
        public bool ShowingModalReminderAttendanceEntry { get; set; }
    }

    public class MapAttendanceDetailItem : CodeWithIdVm
    {
        public string IdAttendanceMapAttendance { get; set; }
        public bool NeedAttachment { get; set; }
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class MapAttendanceTermSession
    {
        public bool NeedValidation { get; set; }

        /// <summary>
        /// If NeedValidation: true, have single value and can have NeedToValidate value.
        /// If NeedValidation: false, have multiple value and can't have NeedToValidate value.
        /// </summary>
        public IEnumerable<MapAttendanceDetailValidation> Validations { get; set; }
    }

    public class MapAttendanceDetailValidation
    {
        public CodeWithIdVm AbsentBy { get; set; }
        public IEnumerable<MapAttendanceValidation> Attendances { get; set; }
    }

    public class MapAttendanceValidation : CodeWithIdVm
    {
        public bool NeedToValidate { get; set; }
    }

    public class Workhabits : CodeWithIdVm
    {
        public string IdWorkhabit { get; set; }
    }
}
