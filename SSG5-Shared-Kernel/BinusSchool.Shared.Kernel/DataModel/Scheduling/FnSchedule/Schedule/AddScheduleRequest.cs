using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule
{
    public class AddScheduleRequest
    {
        public string IdAscTimeTable { get; set; }
        public string IdGrade { get; set; }
        public string IdSession { get; set; }
        public string IdDay { get; set; }
        public int Semester { get; set; }
        public List<AddScheduleData> Schedules { get; set; }
    }

    public class AddScheduleData
    {
        public string IdLesson { get; set; }
        public string IdUser { get; set; }
        public string IdVenue { get; set; }
        public string IdWeekVarianDetail { get; set; }
    }
}
