using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentTotalFamilyDemographicsResult
    {
        public int? Semester { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int TotalFamily { get; set; }
    }
}
