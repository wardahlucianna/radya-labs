using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting
{
    public class GetAvailabilitySettingResult : CodeWithIdVm
    {
        public string Days { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdAcademicYear { get; set; }
        public string Semester { get; set; }
        public List<string> Times { get; set; }
    }
}
