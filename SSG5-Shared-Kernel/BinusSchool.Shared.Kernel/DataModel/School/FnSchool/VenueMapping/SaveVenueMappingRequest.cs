using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueMapping
{
    public class SaveVenueMappingRequest
    {
        public string IdAcademicYear { get; set; }
        public List<VenueMappingData> VenueMappingDatas { get; set; }
    }

    public class VenueMappingData
    {
        public string Description { get; set; }
        public ItemValueVm Venue { get; set; }
        public ItemValueVm Building { get; set; }
        public ItemValueVm Floor { get; set; }
        public ItemValueVm VenueType { get; set; }
        public NameValueVm Owner { get; set; }
        public List<NameValueVm> NeedApproval { get; set; }
        public bool VenueStatus { get; set; }
        public DateTime? LastSaved { get; set; }
    }
}
