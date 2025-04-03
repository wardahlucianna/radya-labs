using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleWithClassIDRequest
    {
        public string ClassID { get; set; }
        public string IdGrade { get; set; }
        public string IdAscTimetable { get; set; }
    }
}
