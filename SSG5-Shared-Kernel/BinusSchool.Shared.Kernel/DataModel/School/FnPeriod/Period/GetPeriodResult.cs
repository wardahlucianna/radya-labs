using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class GetPeriodResult : CodeWithIdVm
    {
        public string Acadyear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public int Semester { get; set; }
    }
}
