using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleLessonsByGradeV2Result
    {
        public string IdLesson { get; set; }
        public string ClassId { get; set; }
        public string Homeroom { get; set; }
        public List<GetScheduleLessonsBySession> Sessions { get; set; }
    }

    public class GetScheduleLessonsBySession : ItemValueVm
    {
        public string Day { get; set; }
    }

}
