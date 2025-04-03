using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class UpdatePublishSurveyRequest
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public string SurveyName { get; set; }
        public PublishSurveyType SurveyType { get; set; }
        public string Description { get; set; }
        public string IdSurveyTemplate { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public string IdPublishSurveyLink { get; set; }
        public string IdSurveyTemplateLink { get; set; }
        public string IdSurveyTemplateChildLink { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsGrapicExtender { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsEntryOneTime { get; set; }
        public PublishSurveySubmissionOption? SubmissionOption { get; set; }
        public string AboveSubmitText { get; set; }
        public string ThankYouMessage { get; set; }
        public string AfterSurveyCloseText { get; set; }
        public List<PublishSurveyRespondent> Respondent { get; set; }
        //public List<PublishSurveyMapping> Mapping { get; set; }
    }


}
