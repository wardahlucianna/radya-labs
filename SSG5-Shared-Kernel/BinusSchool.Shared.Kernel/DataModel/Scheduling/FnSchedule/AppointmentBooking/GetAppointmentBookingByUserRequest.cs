using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingByUserRequest
    {
        public string IdUser { set; get; }
        /// <summary>
        /// Role : Staff/Parent/Teacher
        /// </summary>
        public string Role { set; get; }
        public string IdAcademicYear { set; get; }
    }
}
