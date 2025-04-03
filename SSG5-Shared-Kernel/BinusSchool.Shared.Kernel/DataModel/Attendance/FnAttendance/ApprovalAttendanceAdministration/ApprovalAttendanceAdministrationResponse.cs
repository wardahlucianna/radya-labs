using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration
{
    public class ApprovalAttendanceAdministrationResponse
    {
        public string Id { get; set; }
        public CodeWithIdVm School { get; set; }
        public CodeWithIdVm Role { get; set; }
    }
}
