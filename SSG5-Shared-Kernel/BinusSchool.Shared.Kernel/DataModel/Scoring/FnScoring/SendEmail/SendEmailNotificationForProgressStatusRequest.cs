using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SendEmail
{
    public class SendEmailNotificationForProgressStatusRequest
    {
        public string Coordinator { set; get; }
        public NameValueVm Requestor { set; get; }
        public string RequestorRole { set; get; }
        public string RequestorEmail { set; get; }
        public bool NeedApproval { set; get; }
        public List<EntryProgressStatusForEmail> ProgressStatusList { get; set; }
        public EntryProgressStatusRequest_Body ProgressStatusUpdate { get; set; }
        //public UpdateStudentProgressStatus UpdateStudentProgressStatus { get; set; }
        public DateTime Period { set; get; }
        public LinkFilter Link { set; get; }
    }

    //public class UpdateStudentProgressStatus
    //{
    //    public NameValueVm Student { set; get; }
    //    public ItemValueVm Homeroom { set; get; }
    //    public string OldValue { set; get; }
    //    public string NewValue { set; get; }
    //    public string Reason { set; get; }
    //    public ApprovalModule Module { set; get; }
    //    public string IdTransaction { set; get; }
    //}

    public class EntryProgressStatusForEmail
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Homeroom { set; get; }
        public string StudentProgressDescription { set; get; }
    }

    public class LinkFilter
    {
        //public string UserId { get; set; }
        public ItemValueVm Position { get; set; }
        public ItemValueVm School { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public NameValueVm Student { get; set; }
    }
}
