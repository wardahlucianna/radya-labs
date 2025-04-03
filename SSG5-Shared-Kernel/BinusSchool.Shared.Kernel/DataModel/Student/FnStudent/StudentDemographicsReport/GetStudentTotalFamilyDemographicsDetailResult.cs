using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentTotalFamilyDemographicsDetailResult
    {
        public string IdSiblingGroup { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public string MotherPhone { get; set; }
        public string FatherPhone { get; set; }
        public string MotherEmail { get; set; }
        public string FatherEmail { get; set; }
        public List<GetStudentTotalFamilyDemographicsDetailResult_Student> SiblingData { get; set; }
    }

    public class GetStudentTotalFamilyDemographicsDetailResult_Student
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
}
