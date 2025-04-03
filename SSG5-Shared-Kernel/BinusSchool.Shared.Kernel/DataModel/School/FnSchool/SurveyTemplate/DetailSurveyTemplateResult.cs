using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class DetailSurveyTemplateResult
    {
        public string Id { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public string Title { get; set; }
        public SurveyTemplateLanguage LanguageEnum { get; set; }
        public string Language { get; set; }
    }
}
