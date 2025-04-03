using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentInformationWithStudentStatusResult
    {
        public NameValueVm Student {  get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public GetStudentInformation_StudentStatus StudentStatus { get; set; }

    }
    public class GetStudentInformation_StudentStatus : ItemValueVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public DateTime statusStartDate { get; set; }
    }
}
