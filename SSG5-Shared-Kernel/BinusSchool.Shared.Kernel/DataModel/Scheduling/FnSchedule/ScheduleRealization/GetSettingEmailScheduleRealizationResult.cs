using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetSettingEmailScheduleRealizationResult
    {
        public string To { get; set; }
        public List<DataCc> ListDataCc { get; set; }
    }

    public class DataCc
    {
        public string Id { get; set; }
        public string IdSchool { get; set; }
        public bool IsSetSpecificUser { get; set; }
        public CodeWithIdVm Role { get; set; }
        public ItemValueVm Position { get; set; }
        public ItemValueVm User { get; set; }

    }
}
