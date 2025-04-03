using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class ExportExcelSDRTotalStudentDetailsRequest
    {
        public string ViewCategoryType { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
    }
}
