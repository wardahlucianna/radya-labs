using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class DetailAppointmentBookingParentResult
    {
        public string Id { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string InvitationName { get; set; }
        public ItemValueVm Venue { get; set; }
        public ItemValueVm Teacher { get; set; }
        public string TimeInvitation { get; set; }
        public string FootNote { get; set; }
        public DateTime DateInvitation { get; set; }
        public List<DetailStudents> DetailStudents { get; set; }
    }

    public class DetailStudents
    {
        public string IdHomeroomStudent { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Venue { get; set; }
    }
}
