using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class DeleteSchoolEventInvolvementRequest
    {
        public string IdEvent { get; set; }
        public string UserId { get; set; }
    }
}
