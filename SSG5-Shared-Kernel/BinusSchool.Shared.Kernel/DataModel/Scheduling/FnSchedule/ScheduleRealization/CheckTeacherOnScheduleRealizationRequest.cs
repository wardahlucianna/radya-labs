using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class CheckTeacherOnScheduleRealizationRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
    }
}
