using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetAttendanceStatusBySchoolResult
    {
        public CodeWithIdVm Status { get; set; }
        public bool NeedReason { get; set; }
    }
}
