using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class AddHomeroomRequest
    {
        public string IdAcadyear { get; set; }
        public int Semester { get; set; }
        public string SemesterValue { get; set; }
        public string IdGrade { get; set; }
        public string IdClassroom { get; set; }
        public string IdVenue { get; set; }
        public string IdPathway { get; set; }
        public IEnumerable<string> IdPathwayDetails { get; set; }
        public IEnumerable<HomeroomTeacher> Teachers { get; set; }
    }

    public class HomeroomTeacher
    {
        public string IdTeacher { get; set; }
        public string IdPosition { get; set; }
        public bool HasAttendance { get; set; }
        public bool HasScore { get; set; }
        public bool ShowInReportCard { get; set; }
    }
}
