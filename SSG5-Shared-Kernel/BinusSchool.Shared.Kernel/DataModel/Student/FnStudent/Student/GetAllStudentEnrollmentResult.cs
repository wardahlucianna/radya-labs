using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetAllStudentEnrollmentResult : ItemValueVm
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Classroom { get; set; }
    }
}
