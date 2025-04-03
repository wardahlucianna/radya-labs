using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class AddMessageReplyRequest
    {
        public string IdSender { get; set; }
        public string Content { get; set; }
        public string MessageId { get; set; }
        public string ParentMessageId { get; set; }
        public string IdSchool { get; set; }
        public List<MessageAttachment> Attachments { get; set; }
        public bool IsEdit { get; set; }
    }
}
