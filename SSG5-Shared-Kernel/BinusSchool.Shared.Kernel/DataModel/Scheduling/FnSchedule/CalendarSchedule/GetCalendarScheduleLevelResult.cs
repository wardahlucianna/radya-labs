using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleLevelResult : ItemValueVm
    {
        public List<GetCalendarScheduleGrade> Grade { get; set; }
    }

    public class GetCalendarScheduleGrade : ItemValueVm
    {
        public List<GetCalendarScheduleHomeroom> Homeroom { get; set; }

    }

    public class GetCalendarScheduleHomeroom : ItemValueVm
    {
        public int Semester { get; set; }
        public List<GetCalendarScheduleLesson> Subject { get; set; }
    }

    public class GetCalendarScheduleLesson
    {
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Teacher { get; set; }
        public string IdLesson { get; set; }
        public string ClassId { get; set; }

    }
}
