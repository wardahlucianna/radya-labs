using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetIdLessonByUserResult
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public string IdLesson { get; set; }
        public string PositionCode { get; set; }
        public string ClassId { get; set; }
        public ItemValueVm Subject { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public ItemValueVm Teacher { get; set; }
        public int Semester { get; set; }

    }
}
