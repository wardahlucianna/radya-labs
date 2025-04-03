using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class AddSessionExtracurricularAttendanceRequest
    {
        public string IdExtracurricular { get; set; }
        public DateTime Date { get; set; }
        public ItemValueVm Venue { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
