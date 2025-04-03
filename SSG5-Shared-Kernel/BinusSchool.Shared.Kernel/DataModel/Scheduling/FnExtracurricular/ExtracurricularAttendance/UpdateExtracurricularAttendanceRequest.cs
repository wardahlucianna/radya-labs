using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class UpdateExtracurricularAttendanceRequest
    {
        public string IdGeneratedAttendance { get; set; }
        public DateTime Date { get; set; }
        //public ItemValueVm Venue { get; set; }
        //public string StartTime { get; set; }
        //public string EndTime { get; set; }
        public List<StatusAttendanceData> StatusAttendance { get; set; }
    }

    public class StatusAttendanceData
    {
        public string IdStudent { get; set; }
        public string IdStatusAttendance { get; set; }
        public string Reason { get; set; }
    }
}
