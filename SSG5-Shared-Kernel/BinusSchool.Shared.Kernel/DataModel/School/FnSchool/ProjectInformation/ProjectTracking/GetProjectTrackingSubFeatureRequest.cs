using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingSubFeatureRequest : CollectionRequest
    {
        public string IdFeature { get; set; }
    }
}
