using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetPublishSurveyResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semeter { get; set; }
        public string SurveyType { get; set; }
        public string SurveyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string IdSurveyTemplate { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public SurveyTemplateType SurveyTemplateType { get; set; }
    }
}
