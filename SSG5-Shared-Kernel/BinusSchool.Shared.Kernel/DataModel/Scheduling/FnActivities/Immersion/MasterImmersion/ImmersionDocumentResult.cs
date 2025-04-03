using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class ImmersionDocumentResult_Get : CodeWithIdVm
    {
        public string PosterFileName { get; set; }
        public string PosterLink { get; set; }
        public string BrochureFileName { get; set; }
        public string BrochureLink { get; set; }
    }

    public class ImmersionDocumentResult_GetDetail
    {
        public string IdImmersion { get; set; }
        public string PosterFileName { get; set; }
        public string PosterLink { get; set; }
        public string BrochureFileName { get; set; }
        public string BrochureLink { get; set; }
    }
}
