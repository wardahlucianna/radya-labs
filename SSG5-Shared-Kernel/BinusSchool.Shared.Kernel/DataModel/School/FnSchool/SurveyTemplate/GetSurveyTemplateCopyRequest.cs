using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class GetSurveyTemplateCopyRequest : CollectionSchoolRequest
    {
        public string IdAcademicYearFrom { get; set; }
        public string IdAcademicYearTo { get; set; }
        public SurveyTemplateType Type { get; set; }
    }
}
