using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram
{
    public class SaveRespondAnswerBLPRequest
    {
        public string IdSurveyCategory { get; set; }
        public List<BLP_QuestionAnswer> ListQuestionAnswer { get; set; }
    }

    public class BLP_QuestionAnswer
    {
        public bool IsUpdate { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdSurveyQuestionMapping { get; set; }
        public string IdSurveyAnswerMapping { get; set; }
        public string? IdSurveyPeriod { get; set; }
        public string? IdClearanceWeekPeriod { get; set; }
        public string Description { get; set; }
        public string? FileName { get; set; }
    }

    public class BLP_StudentParentInfo
    {
        public string IdParent { get; set; }
        public string ParentName { get; set; }
        public string ParentRole { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
 }
