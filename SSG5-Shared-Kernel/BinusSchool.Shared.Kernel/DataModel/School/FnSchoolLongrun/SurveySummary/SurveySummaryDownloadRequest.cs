using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchoolLongrun.SurveySummary
{
    public class SurveySummaryDownloadRequest
    {
        public string IdPublishSurvey {  get; set; }
        public string IdUser {  get; set; }
        public string IdSchool {  get; set; }
        public SurveyTemplateType SurveyTemplateType { get; set; }
        public List<string> Role { get; set; }
    }
}
