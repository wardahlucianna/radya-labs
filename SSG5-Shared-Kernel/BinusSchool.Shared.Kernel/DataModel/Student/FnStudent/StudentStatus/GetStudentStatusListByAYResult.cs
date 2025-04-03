using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentStatus
{
    public class GetStudentStatusListByAYResult
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm LatestStudentStatus { get; set; }
        public DateTime LatestStudentStatusStartDate { get; set; }
        public DateTime StartDateAY { get; set; }
        public DateTime EndDateAY { get; set; }
    }
}
