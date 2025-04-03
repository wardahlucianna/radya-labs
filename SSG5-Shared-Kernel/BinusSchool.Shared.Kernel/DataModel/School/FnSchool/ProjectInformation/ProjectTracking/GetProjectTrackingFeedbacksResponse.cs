using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingFeedbacksResponse
    {
        public string IdProjectFeedback { get; set; }
        public ItemValueVm School { get; set; }
        public DateTime RequestDate { get; set; }
        public ItemValueVm Requester { get; set; }
        public string FeatureRequested { get; set; }
        public string Description { get; set; }
        public ItemValueVm Status { get; set; }
        public ItemValueVm RelatedModule { get; set; }
        public ItemValueVm RelatedSubModule { get; set; }
        public List<GetProjectTrackingFeedbacksResponse_SprintPlanned> SprintPlanned { get; set; }
    }

    public class GetProjectTrackingFeedbacksResponse_SprintPlanned
    {
        public string IdProjectFeedbackSprintMapping { get; set; }
        public string Year { get; set; }
        public ItemValueVm Section { get; set; }
        public ItemValueVm SprintName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
