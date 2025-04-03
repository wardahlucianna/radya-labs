using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class DetailPublishSurveyResult : ItemValueVm
    {
        public string IdSurveyTemplate { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public string IdSurveyTemplateLink { get; set; }
        public string IdSurveyTemplateChildLink { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public string SurveyName { get; set; }
        public PublishSurveyType SurveyTypeEnum { get; set; }
        public string SurveyType { get; set; }
        public SurveyTemplateType SurveyTemplateTypeEnum { get; set; }
        public string SurveyTemplateType { get; set; }
        public ItemValueVm SurveyTemplate { get; set; }
        public ItemValueVm PublishSurveyLink { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsGrapicExtender { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsEntryOneTime { get; set; }
        public PublishSurveySubmissionOption? SubmissionOptionEnum { get; set; }
        public string SubmissionOption { get; set; }
        public string AboveSubmitText { get; set; }
        public string ThankYouMessage { get; set; }
        public string AfterSurveyCloseText { get; set; }
        public List<DetailPublishSurveyRespondent> Respondent { get; set; }
    }

    public class DetailPublishSurveyRespondent
    {
        public PublishSurveyRole RoleEnum { get; set; }
        public string Role { get; set; }
        public PublishSurveyOption? OptionEnum { get; set; }
        public string Option { get; set; }
        public List<ItemValueVm> Position { get; set; }
        public List<DetailPublishSurveyRespondentUser> User { get; set; }
        public List<ItemValueVm> Department { get; set; }
        public List<DetailPublishSurveyRespondentGrade> Grade { get; set; }
    }

    public class DetailPublishSurveyRespondentGrade
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public int? Semester { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
    public class DetailPublishSurveyRespondentUser : ItemValueVm
    {
        public ItemValueVm TeacherPosition { get; set; }
    }

    public class DetailPublishSurveyRespondentMapping
    {
        public string IdHomeroomStudent { get; set; }
        public ItemValueVm UserTeacher { get; set; }
        public string IdLesson { get; set; }
    }
}
