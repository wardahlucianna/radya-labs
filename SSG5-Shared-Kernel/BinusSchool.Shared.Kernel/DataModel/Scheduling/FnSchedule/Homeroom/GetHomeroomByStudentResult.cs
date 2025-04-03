using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomByStudentResult : ItemValueVm
    {
        public IEnumerable<string> IdStudents { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Classroom { get; set; }
    }
}
