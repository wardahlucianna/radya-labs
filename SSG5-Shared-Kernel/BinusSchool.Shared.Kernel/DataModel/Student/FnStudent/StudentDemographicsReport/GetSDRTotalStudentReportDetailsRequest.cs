using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetSDRTotalStudentReportDetailsRequest : CollectionRequest
    {
        public string ViewCategoryType { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdType { set; get; }
        public List<string> Grade { set; get; }
        public List<string> Homeroom { set; get; }
    }
}
