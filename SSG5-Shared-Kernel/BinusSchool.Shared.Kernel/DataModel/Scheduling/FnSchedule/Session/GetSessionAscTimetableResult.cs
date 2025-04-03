using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Session
{
    public class GetSessionAscTimetableResult
    {
        public string Id { get; set; }
        public string Grade { get; set; }
        public string Pathway { get; set; }
        public int SessionId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string DaysCode { get; set; }
        public string DaysName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationInMinutes { get; set; }
    }
}
