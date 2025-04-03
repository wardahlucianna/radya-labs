using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class AddMessageReplyResponse
    {
        public string IdMessage { get; set; }
        public string IdSender { get; set; }
        public UserMessageType Type { get; set; }
        public bool IsSetSenderAsSchool { get; set; }
        public string Content { get; set; }
        public bool IsAllowReply { get; set; }
        public DateTime? ReplyStartDate { get; set; }
        public DateTime? ReplyEndDate { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public bool IsDraft { get; set; }
        public string ParentMessageId { get; set; }
        public List<MessageRecepientResponse> Recepients { get; set; }
    }
}
