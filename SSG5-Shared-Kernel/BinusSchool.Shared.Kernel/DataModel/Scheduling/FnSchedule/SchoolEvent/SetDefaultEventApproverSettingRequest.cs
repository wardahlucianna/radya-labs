using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class SetDefaultEventApproverSettingRequest
    {
        public string Id { get; set; }
        public string IdApprover1 { get; set; }
        public string? IdApprover2 { get; set; }
    }
}