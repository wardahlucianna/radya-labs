using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification
{
    public class GetAcadYearByStudentRequest : CollectionSchoolRequest
    {
        public string IdStudent { get; set; }
    }
}