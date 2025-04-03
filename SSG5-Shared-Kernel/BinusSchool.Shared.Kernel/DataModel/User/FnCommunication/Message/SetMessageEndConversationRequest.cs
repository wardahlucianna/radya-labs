using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class SetMessageEndConversationRequest
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public string Reason { get; set; }
    }
}