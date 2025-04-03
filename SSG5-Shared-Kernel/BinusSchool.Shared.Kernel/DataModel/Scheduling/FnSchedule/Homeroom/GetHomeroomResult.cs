using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomResult : CodeWithIdVm
    {
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public string TeacherName { get; set; }
        public string Pathway { get; set; }
        public IEnumerable<CodeWithIdVm> Pathways { get; set; }
        public string Building { get; set; }
        public string Venue { get; set; }
        public HomeroomStudentGender TotalGender { get; set; }
        public HomeroomStudentReligion TotalReligion { get; set; }
    }

    public class HomeroomStudentGender
    {
        public int Male { get; set; }
        public int Female { get; set; }
        public int Other {get;set;}
    }

    public class HomeroomStudentReligion
    {
        public int Islam { get; set; }
        public int Protestan { get; set; }
        public int Katolik { get; set; }
        public int Hindu { get; set; }
        public int Buddha { get; set; }
        public int Khonghucu { get; set; }
    }
}
