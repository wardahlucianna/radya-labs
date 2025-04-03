using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class AddMessageResponse
    {
        public string IdMessage { get; set; }
        public string IdSender { get; set; }
        public UserMessageType Type { get; set; }
        public bool IsSetSenderAsSchool { get; set; }
        public string Subject { get; set; }
        public string IdMessageCategory { get; set; }
        public string Content { get; set; }
        public DateTime? PublishStartDate { get; set; }
        public DateTime? PublishEndDate { get; set; }
        public bool IsAllowReply { get; set; }
        public DateTime? ReplyStartDate { get; set; }
        public DateTime? ReplyEndDate { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public bool IsDraft { get; set; }
        public string ParentMessageId { get; set; }
        public List<MessageRecepientResponse> Recepients { get; set; }
    }

    public class MessageRecepientResponse
    {
        public string IdRecepient { get; set; }
        public string IdMessage { get; set; }
        public MessageFolder MessageFolder { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    public class MessageAttachmentResponse
    {
        public string Id { get; set; }
        public string IdMessage { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }


    }
}
