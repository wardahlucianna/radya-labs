using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class StudentDemographicsReportRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public List<string> Level { set; get; }
        public List<string> Grade { set; get; }
        public List<string> Homeroom { set; get; }
        public string IdType { set; get; }
        public string IdReligion { set; get; }
        public Gender? Gender { set; get; }
        public string ViewCategoryType { get; set; }
        public bool TotalStudent { get; set; }
        public bool TotalStudentDetail { get; set; }
        public bool Religion { get; set; }
        public bool ReligionDetail { get; set; }
    }
}
