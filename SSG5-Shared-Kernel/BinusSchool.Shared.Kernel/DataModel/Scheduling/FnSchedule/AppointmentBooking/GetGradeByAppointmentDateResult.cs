using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetGradeByAppointmentDateResult : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public string NameGrade { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}