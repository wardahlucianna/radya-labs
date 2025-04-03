using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class GetCalendarEventDetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public string Name { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public CodeWithIdVm Role { get; set; }
        public EventAttendanceType Attendance { get; set; }
        public EventOptionType? Option { get; set; }
        public IEnumerable<CodeWithIdVm> Grades { get; set; }
        public IEnumerable<CodeWithIdVm> Departments { get; set; }
        public IEnumerable<EventIntendedForSubjectDetail> Subjects { get; set; }
    }

    public class EventIntendedForSubjectDetail
    {
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public IEnumerable<CodeWithIdVm> Subjects { get; set; }
    }
}