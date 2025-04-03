using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleGradesRequest : CollectionSchoolRequest
    {
        public string IdAscTimetable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
