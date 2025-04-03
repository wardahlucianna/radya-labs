using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetStudentAttendanceResult
    {
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

    }
}
