using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleGradeResult : CodeWithIdVm
    {
        public NameValueVm AscTimetable { get; set; }
        // public CodeWithIdVm Week { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        // public IEnumerable<GenerateScheduleGradeHistory> Histories { get; set; }
    }

    // public class GenerateScheduleGradeHistory
    // {
    //     public string Id { get; set; }
    //     public string Name {get;set;}
    //     public DateTime StartPeriod { get; set; }
    //     public DateTime EndPeriod { get; set; }
    //     public DateTime CreatedDate { get; set; }
    // }
}
