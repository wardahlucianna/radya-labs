using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class GetHomeroomStudentByLevelGradeResult
    {
        public NameValueVm Student { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
}
