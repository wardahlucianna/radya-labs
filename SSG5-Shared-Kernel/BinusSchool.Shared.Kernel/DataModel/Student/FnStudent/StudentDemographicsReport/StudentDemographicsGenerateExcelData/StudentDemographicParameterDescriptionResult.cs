using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData
{
    public class StudentDemographicParameterDescriptionResult
    {
        public string School { get; set; }
        public string AcademicYear { get; set; }
        public int? Semester { get; set; }
        public string ViewCategoryType { get; set; }
    }
}
