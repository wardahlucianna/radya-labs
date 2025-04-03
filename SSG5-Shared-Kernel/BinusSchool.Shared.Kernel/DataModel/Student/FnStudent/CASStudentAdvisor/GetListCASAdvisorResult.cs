using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor
{
    public class GetListCASAdvisorResult : ItemValueVm
    {
        public string BinusianID { get; set; }
        public string FullName { get; set; }
        public bool IsDelete { get; set; }
    }
}
