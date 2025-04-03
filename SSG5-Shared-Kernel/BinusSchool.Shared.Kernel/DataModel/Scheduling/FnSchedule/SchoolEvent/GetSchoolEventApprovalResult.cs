using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventApprovalResult : CodeWithIdVm
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ApprovalSatatus { get; set; }
        public string ApprovalDescription { get; set; }
        public bool CanApprove { get; set; }
        public int TotalApproved { get; set; }
        public int TotalDeclined { get; set; }
        public int TotalWaiting { get; set; }
        public bool IsShowAcademicCalender { get; set; }

    }
}
