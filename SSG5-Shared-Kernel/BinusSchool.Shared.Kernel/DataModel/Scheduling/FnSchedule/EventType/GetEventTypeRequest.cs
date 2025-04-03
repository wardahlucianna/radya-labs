using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EventType
{
    public class GetEventTypeRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
    }
}
