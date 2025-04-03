using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingUsedUserRequest
    {
        public IEnumerable<string> IdUser { get; set; }
    }
}
