using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageDetailResult : ItemValueVm
    {
        public bool CanEndConversation { get; set; }
        public string ReasonEndConversation { get; set; }
        public string ConversationDescription { get; set; }
        public bool CanApprove { get; set; }
        public string ApproveStatus { get; set; }
        public string ApproveReason { get; set; }
        public string ApprovePreviousStatus { get; set; }
        public string ApprovePreviousReason { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }
        public CodeWithIdVm Type { get; set; }
        public string Subject { get; set; }
        public DateTime? PublishStartDate { get; set; }
        public DateTime? PublishEndDate { get; set; }
        public DateTime? ReplyStartDate { get; set; }
        public DateTime? ReplyEndDate { get; set; }
        public DateTime? DateIn { get; set; }
        public string Content { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public bool IsAllowReply { get; set; }
        public bool CanReplied { get; set; }
        public CodeWithIdVm MessageCategory { get; set; }
        public List<MessageRecepientResponse> Recepients { get; set; }
        public List<MessageAttachmentResponse> Attachments { get; set; }
        public string ProfilePicture { get; set; }
        public string IdSender { get; set; }
        public bool IsSetSenderAsSchool { get; set; }
        public string FeedbackType { get; set; }
        public CodeWithIdVm FeedbackTypeDetail { get; set; }
        public List<GetMessageReplyResult> Replies { get; set; }
        public List<GetMessagePreviousResult> Previous { get; set; }
        public string ParentMessageId { get; set; }
        public bool IsSendEmail { get; set; }
        public bool IsUnsend { get; set; }
        public string StatusUnsend { get; set; }
        public bool IsGroupMember { get; set; }
        public List<ListMemberSentTo> GroupMember { get; set; }
        public List<DetailMessageFor> MessageFors{ get; set; }
    }
    public class DetailMessageFor
    {
        public UserRolePersonalOptionRole RoleEnum { get; set; }
        public string Role { get; set; }
        public MessageForOption OptionEnum { get; set; }
        public string Option { get; set; }
        public List<ItemValueVm> Depertements { get; set; }
        public List<ItemValueVm> Positions { get; set; }
        public List<DetailMessageForPersonal> Personal { get; set; }
        public List<DetailMessageForGrade> Grade { get; set; }
    }

    public class DetailMessageForPersonal
    {
        public string IdUser { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
    public class DetailMessageForGrade
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public List<ItemValueVm> Homeroom { get; set; }
        public int? Semester { get; set; }
    }
    public class GetMessageReplyResult
    {
        public string SenderName { get; set; }
        public bool IsRead { get; set; }
        public UserMessageType Type { get; set; }
        public string Subject { get; set; }
        public DateTime? DateIn { get; set; }
        public string Content { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public List<MessageRecepientResponse> Recepients { get; set; }
        public List<MessageAttachmentResponse> Attachments { get; set; }
        public string IdSender { get; set; }
        public string FeedbackType { get; set; }
    }

    public class GetMessagePreviousResult
    {
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public DateTime? DateIn { get; set; }
        public string Content { get; set; }
        public string IdSender { get; set; }
        public string ProfilePicture { get; set; }
        public List<MessageAttachmentResponse> Attachments { get; set; }
    }
}
