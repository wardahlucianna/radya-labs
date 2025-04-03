using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification
{
    public class GetListStudentCertificationRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string Position { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdPeriod { get; set; }
        public string IdHomeroom { get; set; }
        public string ClassId { get; set; }
    }
}
