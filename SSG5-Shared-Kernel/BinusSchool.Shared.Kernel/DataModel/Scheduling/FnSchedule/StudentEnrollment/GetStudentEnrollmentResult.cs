using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentResult
    {
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public int Semester { get; set; }
        public IEnumerable<CodeWithIdVm> LessonGroups { get; set; }
        public IEnumerable<IDictionary<string, object>> Lessons { get; set; }
    }
}