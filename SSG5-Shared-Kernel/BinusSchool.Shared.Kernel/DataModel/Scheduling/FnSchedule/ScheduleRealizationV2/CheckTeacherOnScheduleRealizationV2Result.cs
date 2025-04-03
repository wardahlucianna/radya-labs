using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class CheckTeacherOnScheduleRealizationV2Result
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public bool HaveDailyAttendance { get; set; }
        public bool HaveSessionAttendance { get; set; }
    }
}
