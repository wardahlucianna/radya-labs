
namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule
{
    public class GetScheduleRequest
    {
        public string Search { get; set; }
        public string IdAscTimetable { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public int? Semester { get; set; }
    }
}
