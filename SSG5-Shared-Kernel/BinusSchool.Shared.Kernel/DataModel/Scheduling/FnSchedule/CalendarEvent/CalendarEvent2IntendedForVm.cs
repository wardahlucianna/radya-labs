using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class CalendarEvent2IntendedForVm
    {
        public string Role { get; set; }
        public EventOptionType Option { get; set; }
        public CalendarEvent2ForTeacher ForTeacher { get; set; }
        public CalendarEvent2ForStudent ForStudent { get; set; }
    }

    public class CalendarEvent2ForTeacher
    {
        public IEnumerable<string> IdGrades { get; set; }
        public IEnumerable<string> IdDepartments { get; set; }
        public IEnumerable<CalendarEvent2ForTeacherOptionSubject> Subjects { get; set; }

        public class CalendarEvent2ForTeacherOptionSubject
        {
            public string IdGrade { get; set; }
            public IEnumerable<string> IdSubjects { get; set; }
        }
    }

    public class CalendarEvent2ForStudent
    {
        public IEnumerable<string> IdStudents { get; set; }
        public IEnumerable<string> IdSubjects { get; set; }
        public IEnumerable<string> IdHomerooms { get; set; }
        public bool ShowOnCalendarAcademic { get; set; }
        public EventIntendedForAttendanceStudent AttendanceOption { get; set; }
        public bool SetAttendanceEntry { get; set; }
        public EventIntendedForAttendancePICStudent? PicAttendance { get; set; }
        public string IdUserPic { get; set; }
        public bool? RepeatAttendanceCheck { get; set; }
        public IEnumerable<CalendarEvent2ForStudentAttendanceDate> AttendanceCheckDates { get; set; }
    }

    public class CalendarEvent2ForStudentAttendanceDate
    {
        public DateTime StartDate { get; set; } //change by robby
        public IEnumerable<CalendarEvent2ForStudentAttendanceCheck> AttendanceChecks { get; set; }
    }

    public class CalendarEvent2ForStudentAttendanceCheck
    {
        public string Name { get; set; }
        public int TimeInMinute { get; set; }
        public bool IsMandatory { get; set; }

        public TimeSpan GetTime() => TimeSpan.FromMinutes(TimeInMinute);
    }
}
