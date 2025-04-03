using System.Linq;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentStudentResult : ItemValueVm
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string Homeroom { get; set; }
        public string BinusianId { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
    }
}
