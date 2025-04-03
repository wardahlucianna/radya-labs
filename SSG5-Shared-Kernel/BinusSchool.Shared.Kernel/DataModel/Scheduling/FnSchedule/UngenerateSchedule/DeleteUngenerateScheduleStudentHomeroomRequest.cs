using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleStudentHomeroomRequest
    {
        public string CodeAction { get; set; }
        public string IdAscTimetable { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public IEnumerable<UngenerateScheduleStudentHomeroom> Students { get; set; }
    }
    public class UngenerateScheduleStudentHomeroom
    {
        public string IdStudent { get; set; }
    }
}
