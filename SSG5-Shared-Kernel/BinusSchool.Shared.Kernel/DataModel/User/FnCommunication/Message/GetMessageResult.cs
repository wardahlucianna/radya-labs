using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using System;
using System.Runtime.Serialization;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageResult : ItemValueVm
    {
        public bool EndConversation { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }
        public UserMessageType Type { get; set; }
        public string Subject { get; set; }
        public DateTime? PublishStartDate { get; set; }
        public DateTime? PublishEndDate { get; set; }
        public DateTime? DateIn { get; set; }
        public string Content { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public string Recepients { get; set; }
        public string ProfilePicture { get; set; }
        public string IdSender { get; set; }
        public string FeedbackType { get; set; }
        public string ApprovalStatus { get; set; }
        public bool IsUnsend { get; set; }
        public string UnsendApprovalStatus { get; set; }
        [IgnoreDataMember]
        public string SchoolDescription { get; set; }
        [IgnoreDataMember]
        public string UserId { get; set; }
        [IgnoreDataMember]
        public string UserDisplayName { get; set; }
        [IgnoreDataMember]
        public string ParentMessageId { get; set; }
        [IgnoreDataMember]
        public string MessageSubject { get; set; }
        [IgnoreDataMember]
        public StatusMessage StatusMessage { get; set; }
        [IgnoreDataMember]
        public int? MessageApprovalStateNumber { get; set; }
        [IgnoreDataMember]
        public StatusMessage StatusUnsend { get; set; }
        [IgnoreDataMember]
        public int? UnsendStateNumber { get; set; }
    }
}
