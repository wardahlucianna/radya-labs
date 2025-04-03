using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class ExportExcelSDRReligionReportDetailsRequest
    {
        public string ViewCategoryType { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<string> Grade { set; get; }
        public List<string> Homeroom { set; get; }
        public string IdReligion { set; get; }
        public Gender Gender { set; get; }
    }
}
