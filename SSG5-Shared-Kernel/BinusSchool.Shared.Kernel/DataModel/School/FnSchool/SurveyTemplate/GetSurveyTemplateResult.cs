using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.SurveyTemplate
{
    public class GetSurveyTemplateResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Status { get; set; }
        public string IdTemplateChild { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
