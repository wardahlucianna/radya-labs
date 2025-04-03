using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentSubjectRequest : CollectionRequest
    {
        public string IdHomeroom { get; set; }
    }
}
