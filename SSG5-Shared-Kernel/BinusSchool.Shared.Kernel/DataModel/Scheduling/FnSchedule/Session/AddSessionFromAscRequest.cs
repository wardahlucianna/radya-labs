using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Session
{
    public class AddSessionFromAscRequest
    {
        public string Id { get; set; }
        public string IdSessionSet { get; set; }
        public string SessionId { get; set; }
        public string IdGradePathway { get; set; }
        public string IdDay { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int DurationInMinutes { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
