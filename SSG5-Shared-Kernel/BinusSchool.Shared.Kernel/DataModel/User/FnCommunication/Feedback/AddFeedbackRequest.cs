using BinusSchool.Data.Model.User.FnCommunication.Message;
using System.Collections.Generic;
namespace BinusSchool.Data.Model.User.FnCommunication.Feedback
{
    public class AddFeedbackRequest  
    {
        public string? IdMessage { get; set; }
        public bool IsDraft { get; set; }
        public string FeedbackType { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string IdSchool { get; set; }
        public List<MessageAttachment> Attachments { get; set; }
    }
}
