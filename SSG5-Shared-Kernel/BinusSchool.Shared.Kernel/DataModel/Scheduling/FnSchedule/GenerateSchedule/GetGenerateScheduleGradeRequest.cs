using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleGradeRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public string IdAscTimetable { get; set; }
    }
}