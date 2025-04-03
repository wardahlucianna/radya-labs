using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MySurvey
{
    public class GetMySurveyResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string SurveyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string StatusSurvey { get; set; }
        public string IdSurvey { get; set; }
        public string AfterSurveyCloseText { get; set; }
        public bool IsEntryOneTime { get; set; }
        public string SubmissionOption { get; set; }
        public string LinkPublishSurvey { get; set; }
        public string Language { get; set; }
        public string LanguageLinkPublishSurvey { get; set; }
    }
}
