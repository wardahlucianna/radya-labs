using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class AddLessonCopyRequest
    {
        public string IdAcadyearCopyTo { get; set; }
        public int SemesterCopyTo { get; set; }
        public List<string> IdLesson { get; set; }
    }
}
