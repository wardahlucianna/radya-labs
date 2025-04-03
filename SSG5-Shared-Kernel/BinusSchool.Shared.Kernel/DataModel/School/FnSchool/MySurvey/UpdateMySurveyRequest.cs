using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MySurvey
{
    public class UpdateMySurveyRequest : AddMySurveyRequest
    {
        public string Id { get; set; }
    }
}
