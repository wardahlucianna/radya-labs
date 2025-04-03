using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public enum ImmersionDocumentRequest_ImmersionDocumentType
    {
        poster = 0,
        brochure
    }

    //public class ImmersionDocumentRequest_Post
    //{
    //    public string IdImmersion { get; set; }
    //    public ImmersionDocumentRequest_ImmersionDocumentType ImmersionDocumentType { get; set; }
    //    public bool NewCreatedImmersion { get; set; }
    //}

    public class ImmersionDocumentRequest_Put
    {
        public string IdImmersion { get; set; }
        public string ImmersionDocumentType { get; set; }
        public bool NewCreatedImmersion { get; set; }
    }
    public class ImmersionDocumentRequest_Get
    {
        public IEnumerable<string> IdImmersions { get; set; }
    }

}
