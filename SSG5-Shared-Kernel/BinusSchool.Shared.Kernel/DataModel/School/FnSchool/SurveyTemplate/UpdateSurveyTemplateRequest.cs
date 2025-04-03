using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class UpdateSurveyTemplateRequest
    {
        public string Id { get; set; }
        public string IdTemplateChild { get; set; }
        public string Title { get; set; }
        public SurveyTemplateLanguage Language { get; set; }
        public SurveyTemplateStatus Status { get; set; }
    }
}
