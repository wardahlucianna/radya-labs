using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.MasterWorkhabit
{
    public class GetWorkhabitResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
    }
}
