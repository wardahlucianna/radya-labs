using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleGradeHistoryResult : CodeWithIdVm
    {
        public NameValueVm AscTimetable { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public DateTime? DateIn {get;set;}

    }
}
