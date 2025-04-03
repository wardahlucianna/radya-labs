using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting
{
    public class AddAvailabilitySettingRequest
    {
        public string Day { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUserTeacher { get; set; }
        public int Semester { get; set; }
        public List<AvailabilitySettingRequest> AvailabilitySettings { get; set; }
    }

    public class AvailabilitySettingRequest
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
