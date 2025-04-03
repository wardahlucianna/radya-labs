using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventApprovalRequest : CollectionSchoolRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ApprovalStatus { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
    }
}
