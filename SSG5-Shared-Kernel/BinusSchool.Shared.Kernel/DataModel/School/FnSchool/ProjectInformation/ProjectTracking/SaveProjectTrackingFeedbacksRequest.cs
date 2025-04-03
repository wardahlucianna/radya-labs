using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class SaveProjectTrackingFeedbacksRequest
    {
        public string IdProjectFeedback { get; set; }
        public DateTime RequestDate { get; set; }
        public string IdSchool { get; set; }
        public string Requester { get; set; }
        public string FeatureRequested { get; set; }
        public string Description { get; set; }
        public string IdRelatedModule { get; set; }
        public string IdRelatedSubModule { get; set; }
        public string IdStatus { get; set; }
        public List<SaveProjectTrackingFeedbacksRequest_SprintPlanned> SprintPlanned { get; set; }
    }

    public class SaveProjectTrackingFeedbacksRequest_SprintPlanned
    {
        public string IdProjectPipeline { get; set; }
    }
}
