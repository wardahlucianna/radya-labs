using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class AddHomeroomCopyRequest
    {
        public string IdAcadyearCopyTo { get; set; }
        public int SemesterCopyTo { get; set; }
        public List<string> IdHomeroom { get; set; }
    }
}
