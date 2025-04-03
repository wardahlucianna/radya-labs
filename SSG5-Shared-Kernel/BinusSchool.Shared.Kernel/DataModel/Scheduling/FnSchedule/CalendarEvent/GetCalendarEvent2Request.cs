using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class GetCalendarEvent2Request : CollectionSchoolRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdEventType { get; set; }
        public string IdUser { get; set; }
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Role { get; set; }
        public bool? ExcludeHiddenEvent { get; set; }
        public bool? ExcludeOptionMetadata { get; set; }
    }
}