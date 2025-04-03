using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListRecapResult : CodeWithIdVm
    {
        public string StudentName { get; set; }
        public string BinusianID { get; set; }
        public string HomeroomStudentId { get; set; }
        public string Grade { get; set; }
        public string Class { get; set; }

        public string IdInvitationBooking { get; set; }
        public InvitationBookingInitiateBy InitiateBy { get; set; }
        public CodeWithIdVm Teacher { get; set; }
        public string IdVenue { get; set; }
        public string Venue { get; set; }
        public DateTime StartDateInvitation { get; set; }
        public DateTime EndDateInvitation { get; set; }
        public InvitationBookingStatus Status { get; set; }
        public string Note { get; set; }
        public string IdUserTeacher { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReschedule { get; set; }
    }
}
