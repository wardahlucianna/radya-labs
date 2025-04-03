using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingParentRequest : CollectionSchoolRequest
    {
        public string IdUser { get; set; }
        /// <summary>
        /// Role : STAFF,PARENT
        /// </summary>
        public string Role { get; set; }
        public DateTime? Date { get; set; }
    }
}
