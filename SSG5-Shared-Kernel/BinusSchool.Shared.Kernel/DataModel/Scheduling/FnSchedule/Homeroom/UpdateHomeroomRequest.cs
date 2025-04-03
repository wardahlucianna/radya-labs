using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class UpdateHomeroomRequest
    {
        public string Id { get; set; }
        public string IdVenue { get; set; }
        public IEnumerable<HomeroomTeacher> Teachers { get; set; }
    }
}