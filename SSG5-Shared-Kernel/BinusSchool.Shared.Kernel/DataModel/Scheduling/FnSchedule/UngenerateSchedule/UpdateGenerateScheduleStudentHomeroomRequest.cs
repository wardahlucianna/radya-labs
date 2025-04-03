using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class UpdateGenerateScheduleStudentHomeroomRequest
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
    }
}