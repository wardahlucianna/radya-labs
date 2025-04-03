using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class GetStudentListResult : CodeWithIdVm
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Homeroom { get; set; }
    }
}
