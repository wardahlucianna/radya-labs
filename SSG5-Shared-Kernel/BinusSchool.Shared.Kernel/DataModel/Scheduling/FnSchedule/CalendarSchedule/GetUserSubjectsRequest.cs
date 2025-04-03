using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetUserSubjectsRequest : CollectionRequest
    {
        public string IdUser { get; set; }
        public string IdHomeroom { get; set; }
        public string IdGrade { get; set; }
        public string Position { get; set; }
        public string IdAcadyear { get; set; }
    }
}
