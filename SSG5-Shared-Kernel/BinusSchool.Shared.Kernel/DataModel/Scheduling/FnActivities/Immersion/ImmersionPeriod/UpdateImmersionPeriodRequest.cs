using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod
{
    public class UpdateImmersionPeriodRequest
    {
        public string IdImmersionPeriod { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string Name { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
    }
}
