using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetUserByRolePositionExcludeSubjectResult : CodeWithIdVm
    {
        public string Fullname { get; set; }
        public string BinusianID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
    }
}
