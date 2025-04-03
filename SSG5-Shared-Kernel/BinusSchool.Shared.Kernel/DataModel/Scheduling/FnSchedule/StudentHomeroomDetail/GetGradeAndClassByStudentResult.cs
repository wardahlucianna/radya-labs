using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class GetGradeAndClassByStudentResult
    {
        public string IdStudent { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Classroom { get; set; }
    }
}
