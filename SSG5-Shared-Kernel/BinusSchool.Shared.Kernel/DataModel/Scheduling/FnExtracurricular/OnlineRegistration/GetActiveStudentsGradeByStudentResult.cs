using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetActiveStudentsGradeByStudentResult
    {
        public ItemValueVm School { get; set; }
        public CodeWithIdVm AcadYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public string IdHomeroomStudent { get; set; }
    }
}
