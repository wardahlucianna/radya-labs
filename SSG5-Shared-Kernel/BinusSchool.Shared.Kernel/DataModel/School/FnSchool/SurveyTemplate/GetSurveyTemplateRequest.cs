using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class GetSurveyTemplateRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public SurveyTemplateType Type { get; set; }
    }
}
