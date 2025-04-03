using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetSDRReligionReportsRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public List<string> Level { set; get; }
        public List<string> Grade { set; get; }
        public string ViewCategoryType { get; set; }
    }
}
