using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class AddSurveyTemplateCopyRequest
    {
        public string IdAcademicYearTo { get; set; }
        public List<string> ListIdSurveyTemplate { get; set; }
    }
}
