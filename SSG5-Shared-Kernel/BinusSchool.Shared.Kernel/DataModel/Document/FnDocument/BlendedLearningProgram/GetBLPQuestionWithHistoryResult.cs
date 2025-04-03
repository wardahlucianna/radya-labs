using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram
{
    public class GetBLPQuestionWithHistoryResult
    {
        public string SectionName { get; set; }
        public string Description { get; set; }
        public List<BLPQuestionWithHistory_QuestionAnswerVm> QuestionAnswerList { get; set; }
    }
    public class BLPQuestionWithHistory_QuestionAnswerVm
    {
        public string IdSurveyQuestion { get; set; }
        public string QuestionName { get; set; }
        public string IdSurveyQuestionMapping { get; set; }
        public BLPQuestionWithHistory_BLPAnswerVm AnswerGroup { get; set; }
    }
    public class BLPQuestionWithHistory_BLPAnswerVm
    {
        public int GroupAnswer { get; set; }
        public string AnswerType { get; set; }
        public string ChildsQuestion { get; set; }
        public List<BLPQuestionWithHistory_BLPChildAnswerVm> Answers { get; set; }
    }
    public class BLPQuestionWithHistory_BLPChildAnswerVm
    {
        public string IdSurveyAnswerMapping { get; set; }
        public string IdSurveyQuestionMapping { get; set; }
        public string idSurveyAnswer { get; set; }
        public string SurveyAnswerDesc { get; set; }
        public BLPQuestionWithHistory_BLPStudentAnswerVm StudentAnswer { get; set; }
        public BLPQuestionWithHistory_BLPStudentAnswerHistoryVm StudentAnswerHistory { get; set; }
        public bool isHaveChild { get; set; }
        public BLPQuestionWithHistory_BLPAnswerVm Childs { get; set; }
    }

    public class BLPQuestionWithHistory_BLPStudentAnswerVm
    {

        public string IdSurveyAnswerMapping { get; set; }
        public string SurveyAnswerDesc { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }     
    }
    public class BLPQuestionWithHistory_BLPStudentAnswerHistoryVm
    {

        public string IdSurveyAnswerMapping { get; set; }
        public string SurveyAnswerDesc { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime? ActionDate { get; set; }
    }
}
