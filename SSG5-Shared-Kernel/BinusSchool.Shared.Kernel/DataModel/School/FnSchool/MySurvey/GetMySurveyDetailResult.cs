using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MySurvey
{
    public class GetMySurveyDetailResult : ItemValueVm
    {
        public string IdUser { get; set; }
        public MySurveyStatus Status { get; set; }
        public string IdTemplateSurveyPublish { get; set; }
        public string IdSurveyChild { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string AboveSubmitText { get; set; }
        public string ThankYouMessage { get; set; }
        public string AfterSurveyCloseText { get; set; }
        public bool IsAllInOne { get; set; }
        public List<DetailAllInOne> DetailAllInOnes { get; set; }

        public List<FilledWithOtherFamilies> FilledWithOtherFamilies { get; set; }
    }

    public class DetailAllInOne
    {
        public string IdUser { get; set; }
        public string IdSurvey { get; set; }
        public string IdSurveyChild { get; set; }
        public string IdHomeroomStudent { get; set; }
    }

    public class FilledWithOtherFamilies
    {
        public string IdUser { get; set; }
        public string Name { get; set; }
        public string IdSurvey { get; set; }
        public string IdSurveyChild { get; set; }
        public string IdHomeroomStudent { get; set; }
    }
}
