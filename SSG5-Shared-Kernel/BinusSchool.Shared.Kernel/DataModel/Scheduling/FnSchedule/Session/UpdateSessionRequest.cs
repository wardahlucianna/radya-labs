using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Session
{
    public class UpdateSessionRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int DurationInMinutes { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
