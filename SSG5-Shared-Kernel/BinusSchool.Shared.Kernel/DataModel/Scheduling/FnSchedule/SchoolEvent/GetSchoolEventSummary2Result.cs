using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventSummary2Result
    {
        public string IdEventActivityAward { get; set; }
        public string ParticipantName { get; set; }
        public string BinusianID { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string EventName { get; set; }
        public string Place { get; set; }
        public string Activity { get; set; }
        public string Involvement { get; set; }
        public ICollection<User> PIC { get; set; }
        public ICollection<User> Registratior { get; set; }
        public ICollection<EventDate> EventDates { get; set; }
       
    }

    public class EventDate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class User
    {
        public string IdBinusian { get; set; }
        public string Name { get; set; }
    }
}
