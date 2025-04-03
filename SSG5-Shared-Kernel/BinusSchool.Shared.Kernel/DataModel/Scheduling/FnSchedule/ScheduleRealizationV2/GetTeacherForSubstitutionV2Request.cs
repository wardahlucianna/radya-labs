using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetTeacherForSubstitutionV2Request
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public IEnumerable<string> IdGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsSubstituteTeacher { get; set; }
    }
}
