using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleLessonsByGradeRequest
    {
        public string IdGrade { get; set; }
        public string IdAscTimetable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> IdDays { get; set; }
    }
}
