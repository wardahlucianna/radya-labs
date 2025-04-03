using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleStudentRequest
    {
        public string IdAscTimetable { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string IdGrade { get; set; }
        public IEnumerable<UngenerateScheduleStudent> Students { get; set; }
    }

    public class UngenerateScheduleStudent
    {
        public string IdStudent { get; set; }
        public IEnumerable<string> ClassIds { get; set; }   
    }
}