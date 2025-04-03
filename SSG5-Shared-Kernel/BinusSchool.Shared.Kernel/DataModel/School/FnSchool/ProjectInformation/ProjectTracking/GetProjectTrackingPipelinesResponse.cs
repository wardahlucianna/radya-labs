using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingPipelinesResponse
    {
        public string IdProjectPipeline { get; set; }
        public ItemValueVm Section { get; set; }
        public string SprintName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ItemValueVm Status { get; set; }
        public ItemValueVm Phase { get; set; }
    }
}
