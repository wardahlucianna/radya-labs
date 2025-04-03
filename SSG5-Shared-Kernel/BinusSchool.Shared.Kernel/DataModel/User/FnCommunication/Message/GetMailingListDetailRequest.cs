using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMailingListDetailRequest
    {
        public string IdMailingList { get; set; }
        public string IdSchool { get; set; }
    }
}
