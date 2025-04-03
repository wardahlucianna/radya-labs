using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventInvolvementTeacherResult : CodeWithIdVm
    {
        public string EventId { get; set; }
        public string InvolvementId { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string EventName { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string AwardName { get; set; }
        public string AwardId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ApprovalSatatus { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
