using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class SetMessageApprovalStatusRequest
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public bool IsApproved  { get; set; }
        public string Reason { get; set; }
        public string IdSchool {  get; set; }
    }
}
