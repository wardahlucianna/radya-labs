using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdClassroom { get; set; }
        public string IdPathway { get; set; }
        public string IdVenue { get; set; }
        public string IdTeacher { get; set; }
    }
}
