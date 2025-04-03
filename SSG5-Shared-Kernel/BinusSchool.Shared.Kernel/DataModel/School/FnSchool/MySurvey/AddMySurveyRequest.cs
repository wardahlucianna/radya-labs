using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MySurvey
{
    public class AddMySurveyRequest
    {
        public List<AddMySurveyUser> Users { get; set; }
        public MySurveyStatus Status { get; set; }
        public string IdPublishSurvey { get; set; }
        public string IdSurveyChild { get; set; }
        public string IdSurvey { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public bool IsAllInOne { get; set; }
        public bool IsFilledWithOtherFamilies { get; set; }
        public List<FilledWithOtherFamilies> FilledWithOtherFamilies { get; set; }
    }

    public class AddMySurveyUser
    {
        public string IdUser { get; set; }
        public string IdHomeroomStudent { get; set; }

    }
}
