using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram
{
    public class GetBLPQuestionWithHistoryRequest
    {
        public string IdSchool { get; set; }
        public string IdSurveyCategory { get; set; }
        public string IdStudent { get; set; }
        public string IdSurveyPeriod { get; set; }
        public string IdClearanceWeekPeriod { get; set; }
    }
}
