using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentNationalityDemographyDetailResult
    {
        public string Category { get; set; }
        public List<DataList> ListData { get; set; }
    }

    public class DataList
    {
        public NameValueVm Student { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Pathway { get; set; }
        public string NationalityType { get; set; }
        public string NationalityCountry { get; set; }
        public string HomeroomTeacher { get; set; }
    }
}
