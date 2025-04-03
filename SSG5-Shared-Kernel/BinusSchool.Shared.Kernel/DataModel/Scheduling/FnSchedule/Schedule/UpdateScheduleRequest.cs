
namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule
{
    public class UpdateScheduleRequest
    {
        public string IdSchedule { get; set; }
        public string IdSession { get; set; }
        public string IdDay { get; set; }
        public string IdLesson { get; set; }
        public string IdUser { get; set; }
        public string IdVenue { get; set; }
        public string IdWeekVarianDetail { get; set; }
    }
}
