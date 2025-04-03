using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleLessonsByStudentRequest
    {
        public string IdStudent { get; set; }
        public string IdAscTimetable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
