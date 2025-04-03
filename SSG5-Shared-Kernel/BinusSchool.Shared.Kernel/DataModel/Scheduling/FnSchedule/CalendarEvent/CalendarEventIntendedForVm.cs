using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class CalendarEventIntendedForVm
    {
        public string IdAcadyear { get; set; }
        public string Role { get; set; }
        public EventOptionType Option { get; set; }
        public IEnumerable<string> IdGrades { get; set; }
        public IEnumerable<string> IdDepartments { get; set; }
        public IEnumerable<CalendarEventIntendedForSubject> Subjects { get; set; }

        public CalendarEventIntendedForVm GetIntendedForVm() => new CalendarEventIntendedForVm
        {
            IdAcadyear = IdAcadyear,
            Role = Role,
            Option = Option,
            IdGrades = IdGrades,
            IdDepartments = IdDepartments,
            Subjects = Subjects
        };
    }

    public class CalendarEventIntendedForSubject
    {
        public string IdGrade { get; set; }
        public IEnumerable<string> IdSubjects { get; set; }
    }
}