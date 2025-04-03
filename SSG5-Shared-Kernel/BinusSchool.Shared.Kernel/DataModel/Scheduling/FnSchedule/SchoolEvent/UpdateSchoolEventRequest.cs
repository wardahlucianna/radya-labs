using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class UpdateSchoolEventRequest
    {
        public string IdEvent { get; set; }
        public string EventName { get; set; }
        public string IdAcadyear { get; set; }
        public string IdEventType { get; set; }
        public bool IsShowOnCalendarAcademic { get; set; }
        public bool IsShowOnSchedule { get; set; }
        public string EventObjective { get; set; }
        public string EventPlace { get; set; }
        public EventLevel EventLavel { get; set; }
        public string IdUserCoordinator { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public List<SchoolEventIntendedFor> IntendedFor { get; set; }
        public List<SchoolEventBudget> Budget { get; set; }
        public List<SchoolEventAttachment> AttachmentBudget { get; set; }
        public List<SchoolEventActivity> Activity { get; set; }
        public string IdUserEventApproval1 { get; set; }
        public string IdUserEventApproval2 { get; set; }
        public EventIntendedForAttendanceStudent MandatoryType { get; set; }
        public bool IsAttendanceRepeat { get; set; }
        public EventIntendedForAttendancePICStudent AttandancePIC { get; set; }
        public string AttandancePICIdUser { get; set; }
        public bool IsSetAttendance { get; set; }
        public bool IsPrimaryAttendance { get; set; }
        public string IdUserAwardApproval1 { get; set; }
        public string IdUserAwardApproval2 { get; set; }
        public string IdCertificateTemplate { get; set; }
        public string IdUser { get; set; }
    }
}
