using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailHistoryEventResult
    {
        public string Id { get; set; }
        public DateTime? ChangeDates { get; set; }
        public string ChangeNotes { get; set; }
        public string EventName { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public bool IsShowOnCalendarAcademic { get; set; }
        public bool IsShowOnSchedule { get; set; }
        public string EventObjective { get; set; }
        public string EventPlace { get; set; }
        public EventLevel EventLavel { get; set; }
        public CodeWithIdVm EventCoordinator { get; set; }
        public string IdUserCoordinator { get; set; }
        public List<DetailEventIntendedFor> IntendedFor { get; set; }
        public List<SchoolEventBudget> Budget { get; set; }
        public List<SchoolEventAttachment> AttachmentBudget { get; set; }
        public CodeWithIdVm Role { get; set; }
        public string Option { get; set; }
        public List<EventActivity> Activity { get; set; }
        public CodeWithIdVm Approver1 { get; set; }
        public CodeWithIdVm Approver2 { get; set; }
        public EventIntendedForAttendanceStudent MandatoryType { get; set; }
        public bool IsAttendanceRepeat { get; set; }
        public CodeWithIdVm AwardApprover1 { get; set; }
        public CodeWithIdVm AwardApprover2 { get; set; }
        public DetailCertificateTemplateResult CertificateTemplate { get; set; }
        public DetailForTeacher ForTeacher { get; set; }
        public DetailForStudent ForStudent { get; set; }
        public string StatusEvent { get; set; }
        public Declained StatusDeclined { get; set; }
        public string DescriptionEvent
        { get; set; }
        public string StatusEventAward { get; set; }
        public string DescriptionEventAward { get; set; }
        public bool CanLinkToMerit { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public string DescriptionApprovalEventSetting { get; set; }
        public string DescriptionApprovalRecordOfInvolvement { get; set; }

    }
}
