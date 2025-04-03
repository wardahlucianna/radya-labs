using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification
{
    public class GetAcadYearByStudentResult2
    {
        public CodeWithIdVm Level { get; set; }
        public ICollection<GradeByStudent> Grades { get; set; }
    }

    public class GradeByStudent : CodeWithIdVm
    {
        public CodeWithIdVm Acadyear { get; set; }
    }
}
