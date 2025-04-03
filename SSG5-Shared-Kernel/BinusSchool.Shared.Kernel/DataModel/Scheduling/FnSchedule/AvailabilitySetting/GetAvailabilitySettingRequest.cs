using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting
{
    public class GetAvailabilitySettingRequest
    {
        public string IdUserTeacher { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
    }
}
