using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram
{
    public  class GetBLPQuestionResult
    {
        public string SectionName { get; set; }
        public string Description { get; set; }
        public List<QuestionAnswerVm> QuestionAnswerList { get; set; }       

    }
    public class QuestionAnswerVm
    {
        public string IdSurveyQuestion { get; set; }
        public string QuestionName { get; set; }     
        public string IdSurveyQuestionMapping { get; set; }     
        public BLPAnswerVm AnswerGroup { get; set; }
    }
    public class BLPAnswerVm
    {
        public int GroupAnswer { get; set; }
        public string AnswerType { get; set; }
        public string ChildsQuestion { get; set; }
        public List<BLPChildAnswerVm> Answers { get; set; }
    }
    public class BLPChildAnswerVm 
    {
        public string IdSurveyAnswerMapping { get; set; }        
        public string IdSurveyQuestionMapping { get; set; }
        public string idSurveyAnswer { get; set; }
        public string SurveyAnswerDesc { get; set; }
        public BLPStudentAnswerVm StudentAnswer { get; set; }
        public bool isHaveChild { get; set; }      
        public BLPAnswerVm Childs { get; set; }
    }

    public class BLPStudentAnswerVm
    {

        public string IdSurveyAnswerMapping { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    }
}
