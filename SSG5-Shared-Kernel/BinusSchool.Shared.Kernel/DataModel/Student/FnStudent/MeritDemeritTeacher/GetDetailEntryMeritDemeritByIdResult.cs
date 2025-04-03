using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailEntryMeritDemeritByIdResult : CodeWithIdVm
    {
        public string Id { get; set; }
        public string IdHomeroomStudent { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public DateTime? Date { get; set; }
        public string Category { get; set; }
        public ItemValueVm LevelOfInfraction { get; set; }
        public ItemValueVm MeritDemeritMapping { get; set; }
        public int Point { get; set; }
        public DetailEntryMeritDemeritStudent Student { get; set; }
    }

    public class DetailEntryMeritDemeritStudent
    {
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Note { get; set; }
    }
}
