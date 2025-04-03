using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class GetLessonResult : ItemValueVm
    {
        public string Grade { get; set; }
        public int Semester { get; set; }
        public string ClassId { get; set; }
        public string Subject { get; set; }
        public IEnumerable<NameValueVm> Teachers { get; set; }
        public int TotalPerWeek { get; set; }
        public string Homeroom { get; set; }
    }
}