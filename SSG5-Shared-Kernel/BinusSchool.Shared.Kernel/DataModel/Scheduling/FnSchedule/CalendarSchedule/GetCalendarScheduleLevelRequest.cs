using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleLevelRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string IdLevel { get; set; }
    }
}
