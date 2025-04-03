using System;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageTempResult : ItemValueVm
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
        public string SchoolDescription { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string ParentMessageId { get; set; }
        public string MessageSubject { get; set; }
        public StatusMessage StatusMessage { get; set; }
        public int? MessageApprovalStateNumber { get; set; }
        public StatusMessage StatusUnsend { get; set; }
        public int? UnsendStateNumber { get; set; }
    }

    public class GetMessageRecepientTemp
    {
        public string IdMessage { get; set; }
        public string IdRecepient { get; set; }
        public MessageFolder MessageFolder { get; set; }
        public string DisplayName { get; set; }
        public bool IsRead { get; set; }
    }
}
