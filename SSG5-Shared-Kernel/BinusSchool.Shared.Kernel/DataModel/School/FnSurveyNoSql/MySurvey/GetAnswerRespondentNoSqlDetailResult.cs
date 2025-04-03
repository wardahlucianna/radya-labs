using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSurveyNoSql.MySurvey
{
    public class GetAnswerRespondentNoSqlDetailResult
    {
        public string IdPublishSurvey { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public string IdSurveyTemplateParent { get; set; }
        public string IdSurvey { get; set; }
        public string IdSurveyParent { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string Language { get; set; }
        public string TemplateTitle { get; set; }
        public string SurveyTemplateType { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public List<Section> Sections { get; set; }
    }
}
