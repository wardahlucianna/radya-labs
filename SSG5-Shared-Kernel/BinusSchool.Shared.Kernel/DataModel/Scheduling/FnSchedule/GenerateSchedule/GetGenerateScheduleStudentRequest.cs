using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleStudentRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public int? Semester { get; set; }
    }
}
