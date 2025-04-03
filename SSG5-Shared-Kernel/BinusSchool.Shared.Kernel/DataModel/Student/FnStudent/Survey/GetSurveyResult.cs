using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Survey
{
    public class GetSurveyResult
    {
        public string IdSurvey { set; get; }
        public string SurveyTitle { set; get; }
        public string SurveyMessage { set; get; }
        public int OrderNumber { set; get; }
        public bool IsBlocking { set; get; }
    }
}
