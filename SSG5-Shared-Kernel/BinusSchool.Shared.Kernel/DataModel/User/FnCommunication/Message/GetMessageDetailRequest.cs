using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageDetailRequest : CollectionRequest
    {
        public string IdMessage { get; set; }
        public UserMessageType? Type { get; set; }
        public MessageFolder? MessageFolder { get; set; }
        public string UserId { get; set; }
        public string? IsRead { get; set; }
        public string IdSchool { get; set; }
    }
}
