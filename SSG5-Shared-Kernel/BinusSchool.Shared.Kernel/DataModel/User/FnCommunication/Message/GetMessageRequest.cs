using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageRequest : CollectionRequest
    {
        public UserMessageType? Type { get; set; }
        public MessageFolder MessageFolder { get; set; }
        public string UserId { get; set; }
        public string IsRead { get; set; }
        public string FeedbackType { get; set; }
        public bool IsDraft { get; set; }
        public bool IsApproval { get; set; }
        public StatusMessage? ApprovalStatus { get; set; }
        public string IdMessageCategory { get; set; }
    }
}
