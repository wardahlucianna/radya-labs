using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingParentResult : CodeWithIdVm
    {
        public string Id { get; set; }
        public string AppoitmentName { get; set; }
        public string StudentName { get; set; }
        public string BinusanId { get; set; }
        public DateTime InvitationDate { get; set; }
        public string TimeBooked { get; set; }
        public string Venue { get; set; }
        public string TeacherBooked { get; set; }
    }
}
