using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDayClassIdAndSessionResult 
    {
        public string ClassId { set; get; }
        public List<ItemValueVm> Session { set; get; }
    }
}
