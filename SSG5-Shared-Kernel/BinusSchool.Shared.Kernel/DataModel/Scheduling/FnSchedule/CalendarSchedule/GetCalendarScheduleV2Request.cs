using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleV2Request : CollectionSchoolRequest
    {
        public string IdUserLogin { get; set; }
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdUser { get; set; }
        public string Role { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSubject { get; set; }
        public int? Semester { get; set; }
    }
}
