using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetClassIdByTeacherDateV2Request : ItemValueVm
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
