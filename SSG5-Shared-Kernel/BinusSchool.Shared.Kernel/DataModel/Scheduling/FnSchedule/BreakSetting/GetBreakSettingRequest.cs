using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting
{
    public class GetBreakSettingRequest
    {
        public string IdUserTeacher { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime? DateInvitation { get; set; }
        public DateTime DateCalender { get; set; }
    }
}
