using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingPipelinesRequest : CollectionRequest
    {
        public string Year { get; set; }
        public string IdSection { get; set; }
        public string IdStatus { get; set; }
    }
}
