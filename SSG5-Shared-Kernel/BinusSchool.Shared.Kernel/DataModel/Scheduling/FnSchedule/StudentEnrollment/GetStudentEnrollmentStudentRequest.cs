using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentStudentRequest : CollectionRequest
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public IEnumerable<string> IdSubjects { get; set; }
    }
}
