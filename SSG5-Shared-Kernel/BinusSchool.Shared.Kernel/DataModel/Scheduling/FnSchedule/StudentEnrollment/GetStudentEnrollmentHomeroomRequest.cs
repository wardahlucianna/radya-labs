using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentHomeroomRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
    }
}
