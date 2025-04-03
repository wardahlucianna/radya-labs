using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod
{
    public class GetImmersionPeriodDetailResult
    {
        public string IdImmersionPeriod { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public ItemValueVm Semester { get; set; }
        public string Name { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
    }
}
