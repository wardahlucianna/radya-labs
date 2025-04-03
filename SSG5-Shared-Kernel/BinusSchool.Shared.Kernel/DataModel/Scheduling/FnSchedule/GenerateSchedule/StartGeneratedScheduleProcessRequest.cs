using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class StartGeneratedScheduleProcessRequest
    {
        public string IdSchool { get; set; }
        public List<string> Grades { get; set; }
        public int Version { get; set; }
    }
}
