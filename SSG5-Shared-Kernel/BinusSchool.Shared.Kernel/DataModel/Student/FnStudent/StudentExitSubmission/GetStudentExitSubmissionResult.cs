using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission
{
    public class GetStudentExitSubmissionResult : CodeWithIdVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdUserIn { get; set; }
        public string CreatedBy { get; set; }
        public StatusExitStudent Status { get; set; }
    }
}
