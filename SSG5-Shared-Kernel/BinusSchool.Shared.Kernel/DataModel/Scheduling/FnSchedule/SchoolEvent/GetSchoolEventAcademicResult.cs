using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventAcademicResult : ItemValueVm
    {
        public string Name { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public string IntendedFor { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}