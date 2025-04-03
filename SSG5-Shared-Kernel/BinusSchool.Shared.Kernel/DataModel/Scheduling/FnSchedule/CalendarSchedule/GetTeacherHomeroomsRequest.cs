using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherHomeroomsRequest : CollectionRequest
    {
        public string IdUser { get; set; }
        public string IdAcadYear { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
    }
}
