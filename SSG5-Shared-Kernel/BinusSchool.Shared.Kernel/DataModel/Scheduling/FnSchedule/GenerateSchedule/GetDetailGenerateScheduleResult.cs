using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetDetailGenerateScheduleResult
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Summary { get; set; }
        public CodeWithIdVm Teacher { get; set; }
        public CodeWithIdVm Venue { get; set; }
        public SessionVm Session { get; set; }
        public string Week { get; set; }
        public string ClassId { get; set; }
        public CodeWithIdVm Subject { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
    }

}
