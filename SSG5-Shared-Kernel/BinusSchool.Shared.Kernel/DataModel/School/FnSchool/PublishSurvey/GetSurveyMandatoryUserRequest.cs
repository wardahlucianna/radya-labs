using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetSurveyMandatoryUserRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }
    }
}
