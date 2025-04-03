using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailRecordOfInvolvementResult : CodeWithIdVm
    {
        public string EventName { get; set; }
        public CodeWithIdVm AcademicYear { get; set; } 
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public IEnumerable<DataEventActivity> Activity { get; set; }
        public CodeWithIdVm AwardApprover1 { get; set; }
        public CodeWithIdVm AwardApprover2 { get; set; }
        public CodeWithIdVm CertificateTemplate { get; set; }
        public string ApprovalSatatus { get; set; }
        public Declained StatusDeclined { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

    }
    public class DataEventActivity
    {
        public string Id { get; set; }
        public string IdActivity { get; set; }
        public string NameActivity { get; set; }
        public IEnumerable<DataUserActivity> EventActivityPICIdUser { get; set; }
        public IEnumerable<DataUserActivity> EventActivityRegistrantIdUser { get; set; }
        public List<DetailDataEventActivityAward> EventActivityAwardIdUser { get; set; }
    }

    public class DetailDataEventActivityAward
    {
        public string IdEventActivityAward { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdAward { get; set; }
        public string NameAward { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
    }

    public class DataUserActivity
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}
