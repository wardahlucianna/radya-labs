using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class StartAscTimetableProcessRequest
    {
        public string IdSchool { get; set; }
        public List<string> Grades { get; set; }
    }
}
