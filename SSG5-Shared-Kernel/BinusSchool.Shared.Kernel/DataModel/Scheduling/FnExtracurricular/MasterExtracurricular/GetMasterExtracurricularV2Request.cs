using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularV2Request : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public List<string> IdLevel { get; set; }
        public List<string> IdGrade { get; set; }
        public int? Semester { get; set; }
        public bool? Status { get; set; }
        public string? IdUser { get; set; }
        public string? IdElectiveGroup { get; set; }
        public string? ScheduleDay { get; set; }
    }
}
