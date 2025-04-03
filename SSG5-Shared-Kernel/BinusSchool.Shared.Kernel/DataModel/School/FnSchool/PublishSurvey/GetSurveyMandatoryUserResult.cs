using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetSurveyMandatoryUserResult
    {
        public string Id { get; set; }
        public string SurveyName { get; set; }
        public bool IsMandatory { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StatusSurvey { get; set; }
        public bool IsEntryOneTime { get; set; }
        public List<ChildrenParentData> Respondent { get; set; }
        public string LinkPublishSurvey { get; set; }
        public PublishSurveySubmissionOption? SubmissionOption { get; set; }
        public string Language { get; set; }
        public string LanguageLinkPublishSurvey { get; set; }
        public List<string> Levels { get; set; }
    }

    public class ChildrenParentData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IdSurvey { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
    }
}
