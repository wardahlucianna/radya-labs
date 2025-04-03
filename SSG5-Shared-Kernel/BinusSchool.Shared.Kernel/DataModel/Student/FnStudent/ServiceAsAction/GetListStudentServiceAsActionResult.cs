using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetListStudentServiceAsActionResult
    {
        public ItemValueVm Student { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm OverallStatus { get; set; }
        public string IdServiceAsActionHeader { get; set; }
    }
}
