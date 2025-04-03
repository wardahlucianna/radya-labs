using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleGradeRequest
    {
        public string IdAscTimetable { get; set; }
        public IEnumerable<UngenerateSchedulePeriodGrade> Periods { get; set; }
    }

    public class UngenerateSchedulePeriodGrade
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string IdGrade { get; set; }
        public IEnumerable<string> ClassIds { get; set; }
    }
}  