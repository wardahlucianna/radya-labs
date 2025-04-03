using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailEventSettingResult
    {
        public string Id { get; set; }
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
        public DetailForAll ForAll { get; set; }
        public string StatusEvent { get; set; }
        public Declained StatusDeclined { get; set; }
        public List<Approved> StatusApproved { get; set; }
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

    public class Declained
    {
        public int ApprovalCount { get; set; }
        public string DeclinedBy { get; set; }
        public DateTime? DeclinedDate { get; set; }
        public string Note { get; set; }
    }

    public class Approved
    {
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class CalendarEventTypeVm : CodeWithIdVm
    {
        public string Color { get; set; }
    }

    public class EventActivity
    {
        public string Id { get; set; }
        public string IdActivity { get; set; }
        public string NameActivity { get; set; }
        public CodeWithIdVm DataActivity { get; set; }
        public IEnumerable<UserActivity> EventActivityPICIdUser { get; set; }
        public IEnumerable<UserActivity> EventActivityRegistrantIdUser { get; set; }
        public List<EventActivityAwardDetail> EventActivityAwardIdUser { get; set; }
        public List<EventActivityAwardTeacherDetail> EventActivityAwardTeacherIdUser { get; set; }
    }

    public class UserActivity
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }

    public class EventActivityAwardDetail
    {
        public string IdHomeroomStudent { get; set; }
        public DataStudent DataStudent { get; set; }
        public List<DataAward> DataAward { get; set; }

    }

    public class EventActivityAwardTeacherDetail
    {
        public string IdStaff { get; set; }
        public DataStaff DataStaff { get; set; }
        public List<DataAward> DataAward { get; set; }

    }

    public class DataAward : CodeWithIdVm
    {
        public string IdEventActivityAward { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
    }

    public class DataStudent
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string BinusianID { get; set; }
        public string Grade { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string Homeroom { get; set; }
        public string Involvement { get; set; }
    }

    public class DataStaff
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string BinusianID { get; set; }
        public string SchoolName { get; set; }
        public string SchoolLogo { get; set; }
    }

    public class DetailEventIntendedFor
    {
        public string IdIntendedFor { get; set; }
        public string Role { get; set; }
        public string Option { get; set; }
        public bool SendNotificationToLevelHead { get; set; }
        public bool NeedParentPermission { get; set; }
        public string NoteToParent { get; set; }
        public List<CodeWithIdVm> IntendedForDepartemetIdDepartemet { get; set; }
        public List<CodeWithIdVm> IntendedForPositionIdTeacherPosition { get; set; }
        public List<CodeWithIdVm> IntendedForPersonalIdUser { get; set; }
        public List<IntendedForStudent> IntendedForPersonalIdStudent { get; set; }
        public List<IntendedForParent> IntendedForPersonalIdParent { get; set; }
        public List<IntendedForHomeroomPathway> IntendedForGradeStudentIdHomeroomPathway { get; set; }
        public List<IntendedForHomeroomPathway> IntendedForGradeParentIdHomeroomPathway { get; set; }
        public List<CodeWithIdVm> IntendedForLevelStudentIdLevel { get; set; }
        public List<CodeWithIdVm> IntendedForLevelParentIdLevel { get; set; }
    }

    public class IntendedForParent : CodeWithIdVm
    {
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
    }

    public class IntendedForStudent : CodeWithIdVm
    {
        public string Username { get; set; }
        public string BinusianID { get; set; }
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm Role { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
    }

    public class IntendedForHomeroomPathway : CodeWithIdVm
    {
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public CodeWithIdVm Semester { get; set; }
    }

    public class ForStudentAttendanceDate
    {
        public DateTime StartDate { get; set; }
        public IEnumerable<ForStudentAttendanceCheck> AttendanceChecks { get; set; }
    }

    public class ForStudentAttendanceCheck
    {
        public string Name { get; set; }
        public int TimeInMinute { get; set; }
        public bool IsMandatory { get; set; }

        public TimeSpan GetTime() => TimeSpan.FromMinutes(TimeInMinute);
    }

    public class DetailForTeacher
    {
        public IEnumerable<CodeWithIdVm> Departments { get; set; }
        public IEnumerable<CodeWithIdVm> Positions { get; set; }
        public IEnumerable<CodeWithIdVm> PersonalEvents { get; set; }
    }

    public class DetailForStudent
    {
        public IEnumerable<NameValueVm> Students { get; set; }
        public IEnumerable<DetailForStudentGrade> Grades { get; set; }
        public IEnumerable<CodeWithIdVm> PersonalEvents { get; set; }
        public bool ShowOnCalendarAcademic { get; set; }
        public EventIntendedForAttendanceStudent AttendanceOption { get; set; }
        public bool SetAttendanceEntry { get; set; }
        public EventIntendedForAttendancePICStudent? PicAttendance { get; set; }
        public NameValueVm UserPic { get; set; }
        public bool? RepeatAttendanceCheck { get; set; }
        public IEnumerable<ForStudentAttendanceDate> AttendanceCheckDates { get; set; }

        public class DetailForStudentGrade : CodeWithIdVm
        {
            public CodeWithIdVm Level { get; set; }
            public IEnumerable<DetailHomeroom> Homerooms { get; set; }
        }
    }

    public class DetailSubject
    {
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public DetailHomeroom Homeroom { get; set; }
        public IEnumerable<CodeWithIdVm> Subjects { get; set; }
    }

    public class DetailHomeroom : CodeWithIdVm
    {
        public int Semester { get; set; }
    }

    public class DetailForAll
    {
        public bool ShowOnCalendarAcademic { get; set; }
        public EventIntendedForAttendanceStudent AttendanceOption { get; set; }
        public bool SetAttendanceEntry { get; set; }
        public EventIntendedForAttendancePICStudent? PicAttendance { get; set; }
        public NameValueVm UserPic { get; set; }
        public bool? RepeatAttendanceCheck { get; set; }
        public IEnumerable<ForStudentAttendanceDate> AttendanceCheckDates { get; set; }
    }
}
