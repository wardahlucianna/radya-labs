using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleGradeV2Request
    {
        public string IdAscTimetable { get; set; }
        public IEnumerable<UngenerateSchedulePeriodGradeV2> Periods { get; set; }
    }

    public class UngenerateSchedulePeriodGradeV2
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string IdGrade { get; set; }
        public List<UngenerateScheduleClassIdSession> UngenerateScheduleClass { get; set; }
    }

    public class UngenerateScheduleClassIdSession
    {
        public string ClassId { get; set; }
        public List<string> IdSessions { get; set; }
        public List<string> IdDays { get; set; }
    }
}
