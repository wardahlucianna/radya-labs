using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class GetMapStudentHomeroomDetailResult : GetMapStudentPathwayResult
    {
        public string IdStudent { get; set; }

        public string StudentName { get; set; }
    }
}
