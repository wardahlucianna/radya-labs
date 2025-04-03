using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class MasterStudentDemographicsGenerateExcelRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public List<string> Level { get; set; }
        public List<string> Grade { get; set; }
        public List<string> Homeroom { get; set; }
        public List<string> ReportType { get; set; }
        public bool IsDetail { get; set; }
        public string ViewCategoryType { get; set; }
        public Gender? Gender { get; set; }
        public string IdReportDetailType { get; set; }
    }
}
