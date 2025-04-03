using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetStudentAttendanceRequest : CollectionSchoolRequest
    {
        public string IdHomeroom { get; set; }
        public DateTime Date { get; set; }
        public string ClassId { get; set; }
        public string SessionID { get; set; }

    }
}
