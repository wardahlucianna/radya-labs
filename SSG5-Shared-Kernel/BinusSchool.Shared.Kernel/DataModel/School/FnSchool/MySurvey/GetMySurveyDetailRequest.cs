using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MySurvey
{
    public class GetMySurveyDetailRequest
    {
        public string IdSurvey { get; set; }
        public string IdPublishSurvey { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public int Semester { get; set; }
    }
}
