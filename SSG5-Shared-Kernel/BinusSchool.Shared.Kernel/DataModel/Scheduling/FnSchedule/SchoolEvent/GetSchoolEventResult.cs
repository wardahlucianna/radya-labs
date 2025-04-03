using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent
{
    public class GetSchoolEventResult : CodeWithIdVm
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string AssignedAs { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ApprovalSatatus { get; set; }
        public string ApprovalDescription { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCheckList { get; set; }
        public bool IsAssignedAsCoordinator { get; set; }
    }
}
