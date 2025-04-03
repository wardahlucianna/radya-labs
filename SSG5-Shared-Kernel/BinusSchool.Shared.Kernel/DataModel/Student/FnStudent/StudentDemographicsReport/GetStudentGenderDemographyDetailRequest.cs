using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentGenderDemographyDetailRequest
    {
        public string ViewCategoryType { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public Gender? Gender { get; set; }
        public string GenderInString { get; set; }
        public List<string> Grade { get; set; }
        public List<string> Homeroom { get; set; }
    }
}
