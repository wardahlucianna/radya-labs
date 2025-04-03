using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class GetCalendarEvent2DetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public string Name { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public CodeWithIdVm Role { get; set; }
        public EventOptionType Option { get; set; }
        public CalendarEvent2DetailForTeacher ForTeacher { get; set; }
        public CalendarEvent2DetailForStudent ForStudent { get; set; }
    }

    public class CalendarEvent2DetailForTeacher
    {
        public IEnumerable<CodeWithIdVm> Grades { get; set; }
        public IEnumerable<CodeWithIdVm> Departments { get; set; }
        public IEnumerable<CalendarEvent2DetailSubject> Subjects { get; set; }
    }

    public class CalendarEvent2DetailForStudent
    {
        public IEnumerable<NameValueVm> Students { get; set; }
        public IEnumerable<CalendarEvent2DetailSubject> Subjects { get; set; }
        public IEnumerable<CalendarEvent2DetailForStudentGrade> Grades { get; set; }
        public bool ShowOnCalendarAcademic { get; set; }
        public EventIntendedForAttendanceStudent AttendanceOption { get; set; }
        public bool SetAttendanceEntry { get; set; }
        public EventIntendedForAttendancePICStudent? PicAttendance { get; set; }
        public NameValueVm UserPic { get; set; }
        public bool? RepeatAttendanceCheck { get; set; }
        public IEnumerable<CalendarEvent2ForStudentAttendanceDate> AttendanceCheckDates { get; set; }

        public class CalendarEvent2DetailForStudentGrade : CodeWithIdVm
        {
            public CodeWithIdVm Level { get; set; }
            public IEnumerable<CalendarEvent2DetailHomeroom> Homerooms { get; set; }
        }
    }

    public class CalendarEvent2DetailSubject
    {
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CalendarEvent2DetailHomeroom Homeroom { get; set; }
        public IEnumerable<CodeWithIdVm> Subjects { get; set; }
    }

    public class CalendarEvent2DetailHomeroom : CodeWithIdVm
    {
        public int Semester { get; set; }
    }
}
