using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting
{
    public class DetailAvailabilitySettingResult
    {
        public string IdAcademicYear { get; set; } 
        public string IdUserTeacher { get; set; } 
        public int Semseter { get; set; } 
        public string Day { get; set; } 
        public List<Time> Times { get; set; } 
    }

    public class Time
    {
        public string  StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
