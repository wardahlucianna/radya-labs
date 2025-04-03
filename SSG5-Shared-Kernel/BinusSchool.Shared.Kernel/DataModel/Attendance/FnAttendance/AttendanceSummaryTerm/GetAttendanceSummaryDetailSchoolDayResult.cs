using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSchoolDayResult : CodeWithIdVm
    {
        public GetAttendanceSummaryDetailSchoolDayResult()
        {
            DataAttendanceAndWorkhabit = new List<GetAttendanceAndWorkhabitResult>();
        }

        public DateTime Date { get; set; }
        public ItemValueVm Session { get; set; }
        public string ClassId { get; set; }
        public string Homeroom { get; set; }

        public List<GetAttendanceAndWorkhabitResult> DataAttendanceAndWorkhabit { get; set; }
    }
}
