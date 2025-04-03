using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class GetLessonRequest : CollectionRequest
    {
        public string IdAcadyear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdHomeroom { get; set; }
        public IEnumerable<string> ExceptIds { get; set; }
    }
}
