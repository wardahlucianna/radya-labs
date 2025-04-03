using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class UnpublishSurveyRequest
    {
        public string Id { get; set; }
        public bool IsPublish { get; set; }
    }
}
