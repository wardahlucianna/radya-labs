using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class PublishSurveyRespondentHomeroom : PublishSurveyRespondentGrade
    {
        public string Homeroom { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
    }
}
