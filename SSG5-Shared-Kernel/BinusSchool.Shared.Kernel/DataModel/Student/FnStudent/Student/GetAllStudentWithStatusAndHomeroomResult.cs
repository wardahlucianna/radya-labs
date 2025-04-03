using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetAllStudentWithStatusAndHomeroomResult : ItemValueVm
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public GetAllStudentWithStatusAndHomeroomResult_StudentStatus LatestStudentStatus { get; set; }
    }
    public class GetAllStudentWithStatusAndHomeroomResult_StudentStatus : ItemValueVm
    {
        public DateTime? StartDate { get; set; }
    }
}
