using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class AddSchoolEventRequest
    {
        public string EventName { get; set; }
        public string IdAcadyear { get; set; }
        public string IdEventType { get; set; }
        public bool IsShowOnCalendarAcademic { get; set; }
        public bool IsShowOnSchedule { get; set; }
        public string EventObjective { get; set; }
        public string EventPlace { get; set; }
        public int EventLevel { get; set; }
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
        public List<SchoolEventAttandanceCheck> AttandanceCheck { get; set; }
        public string IdUserAwardApproval1 { get; set; }
        public string IdUserAwardApproval2 { get; set; }
        public string IdCertificateTemplate { get; set; }
    }

    public class SchoolEventIntendedFor
    {
        public string IdIntendedFor { get; set; }
        public string Role { get; set; }
        public EventOptionType Option { get; set; }
        public bool SendNotificationToLevelHead { get; set; }
        public bool NeedParentPermission { get; set; }
        public string NoteToParent { get; set; }
        public List<string> IntendedForDepartemetIdDepartemet { get; set; }
        public List<string> IntendedForPositionIdTeacherPosition { get; set; }
        public List<string> IntendedForPersonalIdUser { get; set; }
        public List<string> IntendedForPersonalIdStudent { get; set; }
        public List<string> IntendedForPersonalIdParent { get; set; }
        public List<string> IntendedForGradeStudentIdHomeroomPathway { get; set; }
        public List<string> IntendedForLevelStudentIdLevel { get; set; }
        public List<string> IntendedForGradeParentIdHomeroomPathway { get; set; }
        public List<string> IntendedForLevelParentIdLevel { get; set; }
    }
    public class SchoolEventBudget
    {
        public string IdBudget { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
    
    public class SchoolEventAttachment
    {
        public string IdAttachmant { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
    }

    public class SchoolEventActivity
    {
        public string Id { get; set; }
        public string IdActivity { get; set; }
        public List<string> EventActivityPICIdUser { get; set; }
        public List<string> EventActivityRegistrantIdUser { get; set; }
        public List<EventActivityAward> EventActivityAwardIdUser { get; set; }
        public List<EventActivityAwardTeacher> EventActivityAwardTeacherIdUser { get; set; }
    }

    public class SchoolEventAttandanceCheck
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Startdate { get; set; }
        public int TimeInMinute { get; set; }
        public bool IsPrimary { get; set; }
        public TimeSpan GetTime() => TimeSpan.FromMinutes(TimeInMinute);
    }

    public class EventActivityAward
    {
        public string IdHomeroomStudent { get; set; }
        public List<string> IdAward { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
    }

    public class EventActivityAwardTeacher
    {
        public string IdStaff { get; set; }
        public List<string> IdAward { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
    }


}
