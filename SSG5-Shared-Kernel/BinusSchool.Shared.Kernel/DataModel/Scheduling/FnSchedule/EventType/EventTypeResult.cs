using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EventType
{
    public class EventTypeResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string Color { get; set; }
        public bool IsUsed { get; set; }
    }
}
