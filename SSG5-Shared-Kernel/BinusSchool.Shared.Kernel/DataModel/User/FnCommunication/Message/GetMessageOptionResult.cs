using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageOptionResult : CodeWithIdVm
    {
        public string Value { get; set; }
        public string IdSchool { get; set; }
    }
}