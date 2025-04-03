namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class SetMessageIsReadRequest
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }
    }
}