using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListBreakSettingRequest : CollectionSchoolRequest
    {
        public string IdInvitationBookingSetting { get; set; }
    }
}
