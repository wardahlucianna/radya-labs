using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetEventScheduleResult
    {
        public string IdEvent { get; set; }
        public string IdScheduleLesson { get; set; }
        public string IdEventSchedule { get; set; }
        public bool IsActive { get; set; }
    }
}
