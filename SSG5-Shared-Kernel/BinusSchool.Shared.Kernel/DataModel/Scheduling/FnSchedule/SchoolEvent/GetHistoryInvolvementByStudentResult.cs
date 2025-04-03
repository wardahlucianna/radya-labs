using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetHistoryInvolvementByStudentResult
    {
        public CodeWithIdVm Grades { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
    }
}
