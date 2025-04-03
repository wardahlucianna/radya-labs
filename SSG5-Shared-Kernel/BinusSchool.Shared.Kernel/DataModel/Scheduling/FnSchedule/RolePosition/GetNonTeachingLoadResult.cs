using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetNonTeachingLoadResult
    {
        public string IdUser { get; set; }
        public ItemValueVmWithOrderNumber Level { get; set; }
        public ItemValueVmWithOrderNumber Grade { get; set; }
        public GetSubjectHomeroom Homeroom { get; set; }
        public GetSubjectLesson Lesson { get; set; }
        public GetSubjectUser Subject { get; set; }
    }
}
