using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentNationalityDemographyRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public List<string> IdLevel { get; set; }
        public List<string> IdGrade { get; set; }
        public string ViewCategoryType { get; set; }
    }
}
