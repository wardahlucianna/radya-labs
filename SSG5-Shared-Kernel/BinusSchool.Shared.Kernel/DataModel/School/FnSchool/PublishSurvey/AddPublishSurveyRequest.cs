using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class AddPublishSurveyRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public int Semester { get; set; }
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

    public class PublishSurveyRespondent
    {
        public PublishSurveyRole Role { get; set; }
        public PublishSurveyOption? Option { get; set; }
        public List<string> IdPosition { get; set; }
        public List<PublishSurveyRespondentUser> User { get; set; }
        public List<string> IdDepartment { get; set; }
        public List<PublishSurveyRespondentGrade> Grade { get; set; }
    }

    public class PublishSurveyRespondentUser
    {
        public string IdUser { get; set; }
        public string IdTeacherPosition { get; set; }
    }
    public class PublishSurveyRespondentGrade
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdHomeroom { get; set; }
    }

    //public class PublishSurveyMapping
    //{
    //    public string IdHomeroomStudent { get; set; }
    //    public string IdUserTeacher { get; set; }
    //    public string IdLesson { get; set; }
    //}
}
