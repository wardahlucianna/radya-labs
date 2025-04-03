using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting
{
    public class AddWeekSettingRequest
    {
        public string IdPeriod { get; set; }
        public string Method { get; set; }
        public int TotalWeek { get; set; }

    }
}
