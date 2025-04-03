using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueMapping
{
    public class GetVenueMappingResult
    {
        public string? Description { get; set; }
        public ItemValueVm Venue { get; set; }
        public ItemValueVm Building { get; set; }
        public NullableItemValueVm? Floor { get; set; }
        public NullableItemValueVm? VenueType { get; set; }
        public NullableNameValueVm? Owner { get; set; }
        public List<NullableNameValueVm>? NeedApproval { get; set; }
        public bool? VenueStatus { get; set; }
        public DateTime? LastSaved { get; set; }
        public string? LastSavedBy { get; set; }

    }

    public class NullableItemValueVm
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
    }

    public class NullableNameValueVm
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
