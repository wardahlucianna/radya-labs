using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentStatus
{
    public class GetLtStudentStatusResult : ItemValueVm
    {
        public string IdStudentStatus { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
    }
}
