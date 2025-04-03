using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListBreakSettingResult : CodeWithIdVm
    {
        public DateTime AppointmentDate { get; set; }
        public string BreakName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public BreakType BreakType { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
    }
}
