using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SendEmail
{
    public class SendEmailApprovalForProgressStatusRequest
    {
        public string Coordinator { set; get; }
        public NameValueVm Requestor { set; get; }
        public string RequestorRole { set; get; }
        public string RequestorEmail { set; get; }
        public bool NeedApproval { set; get; }
        public UpdateStudentProgressStatus UpdateStudentProgressStatus { get; set; }
        public DateTime Period { set; get; }
        public LinkFilter Link { set; get; }
    }

    public class UpdateStudentProgressStatus
    {
        public NameValueVm Student { set; get; }
        public ItemValueVm Homeroom { set; get; }
        public string OldValue { set; get; }
        public string NewValue { set; get; }
        public string Reason { set; get; }
        public ApprovalModule Module { set; get; }
        public string IdTransaction { set; get; }
    }
}
