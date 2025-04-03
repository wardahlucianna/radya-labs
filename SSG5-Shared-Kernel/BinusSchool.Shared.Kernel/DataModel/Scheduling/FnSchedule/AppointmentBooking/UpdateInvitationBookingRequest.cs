using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class UpdateInvitationBookingRequest
    {
        public string Id { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdVenue { get; set; }
        public DateTime? StartDateTimeInvitation { get; set; }
        public DateTime? EndDateTimeInvitation { get; set; }
        public bool IsCancel { get; set; }

        /// <summary>
        /// Role: PARENT,STAFF
        /// </summary>
        public string Role { get; set; }
    }
}
