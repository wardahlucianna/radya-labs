using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerReservationPeriodPolicyResult
    {
        public string IdLockerReservationPeriod { get; set; }
        public ItemValueVm Grade { get; set; }
        public int GradeOrderNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PolicyMessage { get; set; }
    }
}
