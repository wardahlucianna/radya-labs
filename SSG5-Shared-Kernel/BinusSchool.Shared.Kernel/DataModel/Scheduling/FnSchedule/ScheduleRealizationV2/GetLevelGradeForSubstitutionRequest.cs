using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetLevelGradeForSubstitutionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public bool IsLevel { get; set; }
    }
}
