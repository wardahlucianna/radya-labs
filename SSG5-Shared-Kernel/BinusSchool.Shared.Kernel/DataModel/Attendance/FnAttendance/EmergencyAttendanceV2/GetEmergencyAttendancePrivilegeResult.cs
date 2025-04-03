using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyAttendancePrivilegeResult
    {
        public ItemValueVm Homeroom { get; set; }
        public List<CodeWithIdVm> Sessions { get; set; }
        public List<ItemValueVm> Grades { get; set; }
    }
}
