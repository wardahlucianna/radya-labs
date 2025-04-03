using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class AddMoveHomeroomRequest
    {
        public string IdHomeroomOld { get; set; }
        public string IdHomeroomNew { get; set; }
        public IEnumerable<string> IdStudents { get; set; }
    }
}