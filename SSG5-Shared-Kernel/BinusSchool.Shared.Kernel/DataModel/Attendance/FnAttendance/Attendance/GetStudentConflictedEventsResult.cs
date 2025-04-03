using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetStudentConflictedEventsResult
    {
        public string ConflictCode { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public NameValueVm Student { get; set; }
        public List<ConflictedEventResult> ConflictEvents { get; set; }
    }

    public class ConflictedEventResult : ItemValueVm
    {
        public List<EventDate> Dates { get; set; }
        public EventCheck EventCheck { get; set; }
    }
}
