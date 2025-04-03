using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class SaveSettingEmailScheduleRealizationRequest
    {
        public List<DataSettingEmailScheduleRealization> DataSettingEmailScheduleRealizations { get; set; }
    }

    public class DataSettingEmailScheduleRealization
    {
        public string Id { get; set; }
        public string IdSchool { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdUser { get; set; }
        public bool IsSetSpecificUser { get; set; }
    }
}
