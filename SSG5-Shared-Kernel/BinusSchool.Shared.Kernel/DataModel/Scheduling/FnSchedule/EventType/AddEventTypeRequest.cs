using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EventType
{
    public class AddEventTypeRequest : CodeVm
    {
        public string IdAcademicYear { get; set; }
        public string Color { get; set; }
    }
}
