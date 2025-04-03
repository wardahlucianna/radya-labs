using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentTotalFamilyDemographicsDetailRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string ViewCategoryType { get; set; }
        public List<string> Grade { get; set; }
        public List<string> Homeroom { get; set; }
    }
}
