using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetDownloadScheduleRealizationResult
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SessionID { get; set; }
        public TimeSpan SessionStartTime { get; set; }
        public TimeSpan SessionEndTime { get; set; }
        public string Session { get; set; }
        public string ClassID { get; set; }
        public string SubtituteTeacher {get; set; }
        public string AbsentTeacher {get; set; }
        public string VenueName { get; set; }
        public string VenueNameOld { get; set; }
        public string Subject { get; set; }
        public string Homeroom { get; set; }
        public string LogoSchool { get; set; }
        public string NotesForSubstitution { get; set; }
        public bool IsByDate { get; set; }
    }
}
