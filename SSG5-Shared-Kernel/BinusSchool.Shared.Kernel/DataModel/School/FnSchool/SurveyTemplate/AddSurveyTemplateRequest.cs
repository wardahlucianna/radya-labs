using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class AddSurveyTemplateRequest
    {
        public string Id { get; set; }
        public string IdTemplateChild { get; set; }
        public string IdAcademicYear { get; set; }
        public SurveyTemplateLanguage Language { get; set; }
        public string Title { get; set; }
        public SurveyTemplateType Type { get; set; }
        public SurveyTemplateStatus Status { get; set; }
    }
}
