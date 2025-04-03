using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class GetStudentHomeroomDetailResult
    {
        public string StudentId { get; set; }
        public string SchoolId { get; set; }
        public CodeWithIdVm AcadYear { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Class { get; set; }
    }
}
