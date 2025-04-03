using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class GetExtracurricularUnattendanceResult : ItemValueVm
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ExtracurricularName { get; set; }
    }
}
