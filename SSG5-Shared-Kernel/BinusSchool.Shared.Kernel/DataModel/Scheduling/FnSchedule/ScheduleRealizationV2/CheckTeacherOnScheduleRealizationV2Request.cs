using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class CheckTeacherOnScheduleRealizationV2Request
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
    }
}