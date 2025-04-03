using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent
{
    public class GetDefaultEventApproverSettingRequest : CollectionSchoolRequest
    {
        public string IdSchool { get; set; }
    }
}