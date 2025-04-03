using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public bool? Status { get; set; }
        public string? IdUser { get; set; }
        public string? IdElectiveGroup { get; set; }
        public string? ScheduleDay { get; set; }
    }
}
