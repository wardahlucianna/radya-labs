
namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherGradesRequest : CollectionSchoolRequest
    {
        public string IdAcadYear { get; set; }
        public string IdLevel { get; set; }
        public string IdUser { get; set; }
    }
}
