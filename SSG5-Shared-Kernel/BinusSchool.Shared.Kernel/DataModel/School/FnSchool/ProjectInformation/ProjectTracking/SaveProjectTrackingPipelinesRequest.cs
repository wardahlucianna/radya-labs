using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class SaveProjectTrackingPipelinesRequest
    {
        public string IdProjectPipeline { get; set; }
        public string IdSection { get; set; }
        public string SprintName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdStatus { get; set; }
        public string IdPhase { get; set; }
    }
}
