using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetUserForUserVenueResult : CodeWithIdVm
    {
        public string Fullname { get; set; }
        public string BinusianID { get; set; }
        public string Username { get; set; }
        public string IdRole { get; set; }
        public string Role { get; set; }
        public string CodePosition { get; set; }
        public string Position { get; set; }
        public string IdGrade { get; set; }
        public string IdUser { get; set; }
    }
}