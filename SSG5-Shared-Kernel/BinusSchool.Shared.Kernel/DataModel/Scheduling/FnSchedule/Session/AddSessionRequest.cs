using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Session
{
    public class AddSessionRequest
    {
        public string Id {get;set;}
        public string IdSessionSet { get; set; }
        public List<string> IdGradePathway { get; set; }
        public List<string> IdDay { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int DurationInMinutes { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
