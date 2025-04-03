using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class AvailabilityTime
    {
        public string Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan StartTimeTittle { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsUse { get; set; }
    }
}
