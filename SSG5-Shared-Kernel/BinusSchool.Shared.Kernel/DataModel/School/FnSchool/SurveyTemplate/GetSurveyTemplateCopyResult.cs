using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class GetSurveyTemplateCopyResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public bool IsCanCopy { get; set; }
    }
}
