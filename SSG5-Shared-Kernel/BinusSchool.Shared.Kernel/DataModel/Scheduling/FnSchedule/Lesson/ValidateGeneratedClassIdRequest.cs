using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class ValidateGeneratedClassIdRequest
    {
        public string IdAcadyear { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdSubject { get; set; }
        public string ClassIdFormat { get; set; }
        public string ClassIdToValidate { get; set; }
        public List<string> BookedClassIds { get; set; }
    }
}
