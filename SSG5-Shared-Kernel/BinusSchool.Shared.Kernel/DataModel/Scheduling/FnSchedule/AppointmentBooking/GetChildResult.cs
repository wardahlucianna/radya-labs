using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetChildResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string Role { get; set; }
        public string IdSchool { get; set; }
        public CodeWithIdVm Level { get; set; }
    }
}
