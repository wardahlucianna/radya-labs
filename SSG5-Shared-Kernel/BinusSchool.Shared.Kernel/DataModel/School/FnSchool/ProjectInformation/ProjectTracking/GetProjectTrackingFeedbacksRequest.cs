using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingFeedbacksRequest : CollectionRequest
    {
        public string Year { get; set; }
        public string IdSchool { get; set; }
        public string IdStatus { get; set; }
    }
}
