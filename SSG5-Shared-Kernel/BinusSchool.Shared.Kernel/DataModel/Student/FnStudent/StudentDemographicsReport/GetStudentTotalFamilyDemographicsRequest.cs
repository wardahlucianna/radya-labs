using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentTotalFamilyDemographicsRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public List<string> Level { get; set; }
        public List<string> Grade { get; set; }
        public string ViewCategoryType { get; set; }
    }
}
