using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram
{
    public class UploadFileBLPRequest
    {
        public bool IsUpdate { get; set; }
        public string IdStudent { get; set; }
        public string IdSurveyPeriod { get; set; }
        public string? IdClearanceWeekPeriod { get; set; }
        public string IdSurveyQuestionMapping { get; set; }
        public string IdSurveyAnswerMapping { get; set; }
        public string FileName { get; set; }
    }
}
